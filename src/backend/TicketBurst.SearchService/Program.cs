using TicketBurst.Contracts;
using TicketBurst.SearchService;
using TicketBurst.SearchService.Jobs;
using TicketBurst.ServiceInfra;

Console.WriteLine("TicketBurst Search Service starting.");
Console.WriteLine(
    $"Mock DB: {MockDatabase.Venues.All.Count} venues,  " +
    $"{MockDatabase.HallSeatingMaps.All.Count} seating maps, " +
    $"{MockDatabase.Events.All.Count} events.");

using var saleNotificationPublisher = new InProcessMessagePublisher<EventSaleNotificationContract>(
    receiverServiceName: ServiceName.Reservation,
    urlPath: new[] { "notify", "event-sale" });

using var saleStatusUpdateJob = new EventSaleStatusUpdateJob(saleNotificationPublisher);

var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-search",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: 3001,
    commandLineArgs: args);

httpEndpoint.Run();
