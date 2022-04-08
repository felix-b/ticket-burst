using Microsoft.AspNetCore.DataProtection;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ReservationService.Integrations.ProtoActor;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;
using TicketBurst.ServiceInfra.Aws;

Console.WriteLine("TicketBurst Reservation Service starting.");

var isAwsEnvironment = args.Contains("--aws");
var httpPort = args.Contains("--http-port")
    ? Int32.Parse(args[Array.IndexOf(args, "--http-port") + 1])
    : 3002;
// var actorPort = args.Contains("--actor-port")
//     ? Int32.Parse(args[Array.IndexOf(args, "--actor-port") + 1])
//     : 0;

K8sClusterInfoProvider info = new K8sClusterInfoProvider();

ISecretsManagerPlugin secretsManager = isAwsEnvironment
    ? new AwsSecretsManagerPlugin()
    : new DevboxSecretsManagerPlugin(); 
var entityRepo = args.Contains("--mock-db")
    ? UseMockDatabase()
    : UseRealDatabase();
var dataProtectionProvider = isAwsEnvironment
    ? UseAwsKms()
    : null; 
using var notificationPublisher = new InProcessMessagePublisher<EventAreaUpdateNotificationContract>(
    receiverServiceName: ServiceName.Search,
    urlPath: new[] { "notify", "event-area-update" }); 

var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-reservation",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: httpPort,
    commandLineArgs: args,
    dataProtectionProvider: dataProtectionProvider,
    configure: builder => {
        builder.Services.AddSingleton<EventWarmupJob>();
        builder.Services.AddSingleton<IReservationEntityRepository>(entityRepo);
        builder.Services.AddSingleton<IActorEngine>(new EventAreaManagerInProcessCache(entityRepo));
        //builder.Services.AddSingleton<IActorEngine, ProtoActorEngine>(provider => new ProtoActorEngine(provider, actorPort));
        builder.Services.AddSingleton<ReservationExpiryJob>();
    });

var services = httpEndpoint.Services;
var actorEngine = services.GetRequiredService<IActorEngine>();
using var warmupJop = services.GetRequiredService<EventWarmupJob>();
using var reservationExpiryJob = services.GetRequiredService<ReservationExpiryJob>();
using var notificationJob = new EventAreaUpdateNotificationJob(actorEngine, notificationPublisher);

Console.WriteLine("Starting actor cluster.");
actorEngine.StartAsync().Wait();

Console.WriteLine("Starting HTTP endpoint.");
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
