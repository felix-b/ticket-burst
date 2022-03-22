using TicketBurst.SearchService;
using TicketBurst.ServiceInfra;

Console.WriteLine("TicketBurst Search Service starting.");
Console.WriteLine(
    $"Mock DB: {MockDatabase.Venues.All.Count} venues,  " +
    $"{MockDatabase.HallSeatingMaps.All.Count} seating maps, " +
    $"{MockDatabase.Events.All.Count} events.");

var app = ServiceBootstrap.CreateAspNetCoreEndpoint(
    serviceName: "ticketburst-services-search",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: 3001,
    commandLineArgs: args);

app.Run();
