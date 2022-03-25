using TicketBurst.ReservationService;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ServiceInfra;

Console.WriteLine("TicketBurst Search Service starting.");
Console.WriteLine($"Mock DB: {MockDatabase.ReservationJournal.All.Count} audit records.");

var app = ServiceBootstrap.CreateAspNetCoreEndpoint(
    serviceName: "ticketburst-services-reservation",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: 3002,
    commandLineArgs: args);

using var reservationExpiryJob = new ReservationExpiryJob(EventAreaManagerCache.SingletonInstance);
using var areaCapacityPublisher = new ActorNotificationPublisher(EventAreaManagerCache.SingletonInstance);

app.Run();

