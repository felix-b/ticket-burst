using TicketBurst.Contracts;
using TicketBurst.SearchService;
using TicketBurst.SearchService.Integrations;
using TicketBurst.SearchService.Jobs;
using TicketBurst.SearchService.Logic;
using TicketBurst.ServiceInfra;

Console.WriteLine("TicketBurst Search Service starting.");

var searchEntityRepo = args.Contains("--mock-db")
    ? UseMockDatabase()
    : UseRealDatabase();
    
var eventSeatingCache = new EventSeatingStatusCache(searchEntityRepo);

using var saleNotificationPublisher = new InProcessMessagePublisher<EventSaleNotificationContract>(
    receiverServiceName: ServiceName.Reservation,
    urlPath: new[] { "notify", "event-sale" });

using var saleStatusUpdateJob = new EventSaleStatusUpdateJob(searchEntityRepo, saleNotificationPublisher);

var httpEndpoint = ServiceBootstrap.CreateHttpEndpoint(
    serviceName: "ticketburst-services-search",
    serviceDescription: "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    listenPortNumber: 3001,
    commandLineArgs: args,
    configure: builder => {
        builder.Services.AddSingleton<ISearchEntityRepository>(searchEntityRepo);
        builder.Services.AddSingleton(eventSeatingCache);
    });

httpEndpoint.Run();

ISearchEntityRepository UseMockDatabase()
{
    Console.WriteLine(
        $"Using MOCK DB: {MockDatabase.Venues.All.Count} venues,  " +
        $"{MockDatabase.HallSeatingMaps.All.Count} seating maps, " +
        $"{MockDatabase.Events.All.Count} events.");
    return new InMemorySearchEntityRepository();
}

ISearchEntityRepository UseRealDatabase()
{
    Console.WriteLine($"Using MONGODB");
    MockDatabase.UseObjectId();
    
    var repo = new MongoDbSearchEntityRepository();
    if (repo.ShouldInsertInitialData())
    {
        Console.WriteLine(
            $"Detected empty DB! Inserting data: {MockDatabase.Venues.All.Count} venues, " +
            $"{MockDatabase.HallSeatingMaps.All.Count} seating maps, " +
            $"{MockDatabase.Events.All.Count} events.");
        repo.InsertInitialData();
    }

    return repo;
}

