using Microsoft.AspNetCore.DataProtection;
using TicketBurst.Contracts;
using TicketBurst.ReservationService;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;
using TicketBurst.ServiceInfra.Aws;

Console.WriteLine("TicketBurst Reservation Service starting.");

var isAwsEnvironment = args.Contains("--aws");

ISecretsManagerPlugin secretsManager = isAwsEnvironment
    ? new AwsSecretsManagerPlugin()
    : new DevboxSecretsManagerPlugin(); 
var entityRepo = args.Contains("--mock-db")
    ? UseMockDatabase()
    : UseRealDatabase();
var dataProtectionProvider = isAwsEnvironment
    ? UseAwsKms()
    : null; 

var mockActorEngine = new EventAreaManagerInProcessCache(entityRepo);

using var reservationExpiryJob = new ReservationExpiryJob(mockActorEngine);
using var notificationPublisher = new InProcessMessagePublisher<EventAreaUpdateNotificationContract>(
    receiverServiceName: ServiceName.Search,
    urlPath: new[] { "notify", "event-area-update" }); 

using var notificationJob = new EventAreaUpdateNotificationJob(
    mockActorEngine,
    notificationPublisher);

using var warmupJop = new EventWarmupJob(mockActorEngine);

var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-reservation",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: 3002,
    commandLineArgs: args,
    dataProtectionProvider: dataProtectionProvider,
    configure: builder => {
        builder.Services.AddSingleton<EventWarmupJob>(warmupJop);
        builder.Services.AddSingleton<IReservationEntityRepository>(entityRepo);
        builder.Services.AddSingleton<IActorEngine>(mockActorEngine);
    });

httpEndpoint.Run();

IDataProtectionProvider UseAwsKms()
{
    Console.WriteLine("Using AWS KMS.");
    return new AwsKmsDataProtectionProvider();
}

IReservationEntityRepository UseRealDatabase()
{
    Console.WriteLine("Using MONGODB.");
    var connectionString = secretsManager.GetConnectionStringSecret("reservation-db-connstr").Result;
    var repo = new MongoDbReservationEntityRepository(connectionString);
    return repo;
}

IReservationEntityRepository UseMockDatabase()
{
    Console.WriteLine("Using MOCK DB.");
    return new InMemoryReservationEntityRepository();
}
