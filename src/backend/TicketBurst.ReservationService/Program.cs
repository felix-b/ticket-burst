using TicketBurst.Contracts;
using TicketBurst.ReservationService;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;

Console.WriteLine("TicketBurst Reservation Service starting.");

var entityRepo = args.Contains("--mock-db")
    ? UseMockDatabase()
    : UseRealDatabase();

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
    configure: builder => {
        builder.Services.AddSingleton<EventWarmupJob>(warmupJop);
        builder.Services.AddSingleton<IReservationEntityRepository>(entityRepo);
        builder.Services.AddSingleton<IActorEngine>(mockActorEngine);
    });

httpEndpoint.Run();


IReservationEntityRepository UseRealDatabase()
{
    Console.WriteLine("Using MONGODB.");
    return new MongoDbReservationEntityRepository();
}

IReservationEntityRepository UseMockDatabase()
{
    Console.WriteLine("Using MOCK DB.");
    return new InMemoryReservationEntityRepository();
}

