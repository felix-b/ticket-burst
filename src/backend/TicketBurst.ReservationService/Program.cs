using TicketBurst.Contracts;
using TicketBurst.ReservationService;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;

Console.WriteLine("TicketBurst Reservation Service starting.");
Console.WriteLine($"Mock DB: {MockDatabase.ReservationJournal.All.Count} reservation journal records.");

using var reservationExpiryJob = new ReservationExpiryJob(EventAreaManagerCache.SingletonInstance);

using var notificationPublisher = new InProcessMessagePublisher<EventAreaUpdateNotificationContract>(
    receiverServiceName: ServiceName.Search,
    urlPath: new[] { "notify", "event-area-update" }); 

using var notificationJob = new EventAreaUpdateNotificationJob(
    EventAreaManagerCache.SingletonInstance,
    notificationPublisher);

using var warmupJop = new EventWarmupJob(EventAreaManagerCache.SingletonInstance);

var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-reservation",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: 3002,
    commandLineArgs: args,
    configure: builder => {
        builder.Services.AddSingleton<EventWarmupJob>(warmupJop);
    });

httpEndpoint.Run();
