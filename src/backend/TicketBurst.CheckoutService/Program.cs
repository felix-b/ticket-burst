// ReSharper disable AccessToDisposedClosure

using Microsoft.AspNetCore.DataProtection;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;
using TicketBurst.ServiceInfra.Aws;

Console.WriteLine("TicketBurst Checkout Service starting.");

var isAwsEnvironment = args.Contains("--aws");

var dataProtectionProvider = isAwsEnvironment
    ? UseAwsKms()
    : null;
ISecretsManagerPlugin secretsManager = isAwsEnvironment
    ? new AwsSecretsManagerPlugin()
    : new DevboxSecretsManagerPlugin(); 
var entityRepository = args.Contains("--mock-db")
    ? UseMockDatabase()
    : UseRealDatabase();

var mockEmailGateway = new MockEmailGatewayPlugin();
var mockPaymentGateway = new MockPaymentGatewayPlugin();
var mockStorageGateway = new MockStorageGatewayPlugin();
var mockSagaEngine = new MockSagaEnginePlugin();

using var orderStatusUpdatePublisher = new InProcessMessagePublisher<OrderStatusUpdateNotificationContract>(
    receiverServiceName: ServiceName.Reservation,
    urlPath: new[] { "notify", "order-status-update" });

var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-checkout",
    serviceDescription: "Handles checkout process, payment integration, and ticket delivery.",
    listenPortNumber: 3003,
    commandLineArgs: args,
    dataProtectionProvider: dataProtectionProvider,
    configure: builder => {
        builder.Services.AddSingleton<ICheckoutEntityRepository>(entityRepository);
        builder.Services.AddSingleton<IMessagePublisher<OrderStatusUpdateNotificationContract>>(orderStatusUpdatePublisher);
        builder.Services.AddSingleton<ISagaEnginePlugin>(mockSagaEngine);
        builder.Services.AddSingleton<IStorageGatewayPlugin>(mockStorageGateway);
        builder.Services.AddSingleton<IEmailGatewayPlugin>(mockEmailGateway);
        builder.Services.AddSingleton<IPaymentGatewayPlugin>(mockPaymentGateway);
    });

httpEndpoint.Run();

ICheckoutEntityRepository UseRealDatabase()
{
    Console.WriteLine($"Using MYSQL DB.");
    return new MySqlCheckoutEntityRepository(secretsManager);
}

ICheckoutEntityRepository UseMockDatabase()
{
    Console.WriteLine("Using MOCK DB.");
    return new InMemoryCheckoutEntityRepository();
}

IDataProtectionProvider UseAwsKms()
{
    Console.WriteLine("Using AWS KMS.");
    return new AwsKmsDataProtectionProvider();
}
