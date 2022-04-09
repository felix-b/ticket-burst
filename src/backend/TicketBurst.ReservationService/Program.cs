using Microsoft.AspNetCore.DataProtection;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;
using TicketBurst.ServiceInfra.Aws;

Console.WriteLine("TicketBurst Reservation Service starting.");
Console.WriteLine($"K8s? {(K8sClusterInfoProvider.IsK8sEnvironment() ? "YES" : "NO")}");

var isAwsEnvironment = args.Contains("--aws");
var httpPort = args.Contains("--http-port")
    ? Int32.Parse(args[Array.IndexOf(args, "--http-port") + 1])
    : 3002;
var actorPort = args.Contains("--actor-port")
    ? Int32.Parse(args[Array.IndexOf(args, "--actor-port") + 1])
    : 8090;
var memberIndex = args.Contains("--member-index")
    ? Int32.Parse(args[Array.IndexOf(args, "--member-index") + 1])
    : 0;

using IClusterInfoProvider clusterInfoProvider = isAwsEnvironment
    ? new K8sClusterInfoProvider(serviceName: "ticketburst-reservation", namespaceName: "default")
    : new DevboxClusterInfoProvider(actorPort);

var devboxInfo = clusterInfoProvider as DevboxClusterInfoProvider;
await using var devboxClusterOrchestrator = (devboxInfo != null && memberIndex == 0 && !isAwsEnvironment)
    ? new DevboxClusterOrchestrator(actorPort, devboxInfo)
    : null;

if (devboxInfo != null && devboxClusterOrchestrator == null && !isAwsEnvironment)
{
    devboxInfo.StartPollingForChanges();
}

using var statefulClusterMember = new SimpleStatefulClusterMember(clusterInfoProvider, enablePolling: true);

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
        builder.Services.AddSingleton<IClusterInfoProvider>(clusterInfoProvider);
        builder.Services.AddSingleton(statefulClusterMember);
        if (devboxClusterOrchestrator != null)
        {
            builder.Services.AddSingleton(devboxClusterOrchestrator);
        }
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
