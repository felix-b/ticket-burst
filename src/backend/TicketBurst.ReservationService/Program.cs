using Microsoft.AspNetCore.DataProtection;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ReservationService.Integrations.SimpleSharding;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;
using TicketBurst.ServiceInfra.Aws;

Console.WriteLine("TicketBurst Reservation Service starting.");
Console.WriteLine($"K8s? {(K8sClusterInfoProvider.IsK8sEnvironment() ? "YES" : "NO")}");

var isAwsEnvironment = args.Contains("--aws");
var listenPortNumber = args.Contains("--listen-port")
    ? Int32.Parse(args[Array.IndexOf(args, "--listen-port") + 1])
    : 3002;
var memberIndex = args.Contains("--member-index")
    ? Int32.Parse(args[Array.IndexOf(args, "--member-index") + 1])
    : 0;
var memberCount = args.Contains("--member-count")
    ? Int32.Parse(args[Array.IndexOf(args, "--member-count") + 1])
    : 1;

using IClusterInfoProvider clusterInfoProvider = isAwsEnvironment
    ? new K8sClusterInfoProvider(serviceName: "ticketburst-reservation", namespaceName: "default")
    : new DevboxClusterInfoProvider(memberIndex, memberCount);

var devboxInfo = clusterInfoProvider as DevboxClusterInfoProvider;
await using var devboxClusterOrchestrator = (devboxInfo != null && memberIndex == 0 && !isAwsEnvironment)
    ? new DevboxClusterOrchestrator(devboxInfo)
    : null;

if (devboxInfo != null && devboxClusterOrchestrator == null && !isAwsEnvironment)
{
    devboxInfo.StartPollingForChanges();
}

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

using var statefulClusterMember = new SimpleStatefulClusterMember(clusterInfoProvider, enablePolling: true);
await using var inprocActorCache = new EventAreaManagerInProcessCache(entityRepo);
await using var actorMailbox = new SimpleShardMailbox(shardCount: 100, shardCapacity: 1000, inprocActorCache);
await using var shardActorEngine = new SimpleShardActorEngine(
    clusterInfoProvider, 
    statefulClusterMember, 
    actorMailbox,
    inprocActorCache);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-reservation",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: listenPortNumber,
    commandLineArgs: args,
    dataProtectionProvider: dataProtectionProvider,
    configure: builder => {
        builder.Services.AddSingleton<EventWarmupJob>();
        builder.Services.AddSingleton<IReservationEntityRepository>(entityRepo);
        builder.Services.AddSingleton<IActorEngine>(shardActorEngine);
        //builder.Services.AddSingleton<IActorEngine, ProtoActorEngine>(provider => new ProtoActorEngine(provider, actorPort));
        builder.Services.AddSingleton<ReservationExpiryJob>();
        builder.Services.AddSingleton<IClusterInfoProvider>(clusterInfoProvider);
        builder.Services.AddSingleton(statefulClusterMember);
        if (devboxClusterOrchestrator != null)
        {
            builder.Services.AddSingleton(devboxClusterOrchestrator);
        }

        builder.Services.AddGrpc();
    }
);

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
