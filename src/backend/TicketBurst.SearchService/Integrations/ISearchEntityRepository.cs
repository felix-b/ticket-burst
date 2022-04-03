using System.Collections.Immutable;
using MongoDB.Driver;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Integrations;

public interface ISearchEntityRepository : IEntityRepository
{
    string MakeNewId();

    Task<IAsyncEnumerable<EventContract>> GetAllEvents();
    Task<IAsyncEnumerable<EventContract>> QueryEvents(EventSearchRequestContract query);
    Task<EventContract?> TryGetEventById(string eventId);
    Task UpdateIsOpenForSale(string eventId, bool newValue);

    Task<IAsyncEnumerable<GenreContract>> GetAllGenres();
    Task<GenreContract?> TryGetGenreById(string genreId);

    Task<IAsyncEnumerable<ShowTypeContract>> GetAllShowTypes();
    Task<ShowTypeContract?> TryGetShowTypeById(string showTypeId);

    Task<IAsyncEnumerable<ShowContract>> GetAllShows();
    Task<ShowContract?> TryGetShowById(string showId);

    Task<IAsyncEnumerable<TroupeContract>> GetAllTroupes();
    Task<IAsyncEnumerable<TroupeContract>> GetTroupesByIds(IEnumerable<string> troupeIds);

    Task<IAsyncEnumerable<VenueContract>> GetAllVenuesWithoutHalls();
    Task<VenueContract?> TryGetVenueById(string venueId);

    Task<IAsyncEnumerable<HallSeatingMapContract>> GetAllHallSeatingMapsWithoutAreas();
    Task<HallSeatingMapContract?> TryGetHallSeatingMapById(string hallSeatingMapId);

    public async Task<EventContract> GetEventByIdOrThrow(string eventId) =>
        await TryGetEventById(eventId)
        ?? throw NotFound<EventContract>(eventId);

    public EventContract GetEventByIdOrThrowSync(string eventId)
    {
        var task = GetEventByIdOrThrow(eventId);
        task.Wait();
        return task.Result;
    }

    public async Task<VenueContract> GetVenueByIdOrThrow(string venueId) =>
        await TryGetVenueById(venueId)
        ?? throw NotFound<VenueContract>(venueId);

    public async Task<ShowContract> GetShowByIdOrThrow(string showId) =>
        await TryGetShowById(showId)
        ?? throw NotFound<ShowContract>(showId);

    public async Task<ShowTypeContract> GetShowTypeByIdOrThrow(string showTypeId) =>
        await TryGetShowTypeById(showTypeId)
        ?? throw NotFound<ShowTypeContract>(showTypeId);

    public async Task<GenreContract> GetGenreByIdOrThrow(string genreId) =>
        await TryGetGenreById(genreId)
        ?? throw NotFound<GenreContract>(genreId);

    public async Task<HallSeatingMapContract> GetHallSeatingMapByIdOrThrow(string hallSeatingMapId) =>
        await TryGetHallSeatingMapById(hallSeatingMapId)
        ?? throw NotFound<HallSeatingMapContract>(hallSeatingMapId);

    public HallSeatingMapContract GetHallSeatingMapByIdOrThrowSync(string hallSeatingMapId)
    {
        var task = GetHallSeatingMapByIdOrThrow(hallSeatingMapId);
        task.Wait();
        return task.Result;
    }

    public IList<EventContract> GetAllEventsSync()
    {
        var task = GetAllEvents();
        task.Wait();
        return task.Result.ToListSync();
    }

    HallSeatingMapContract? TryGetHallSeatingMapByIdSync(string hallSeatingMapId)
    {
        var task = TryGetHallSeatingMapById(hallSeatingMapId);
        task.Wait();
        return task.Result;
    }

    public async Task<HallSeatingMapContract?> TryGetHallSeatingMapWithoutSeats(string hallSeatingMapId)
    {
        var hallSeatingMap = await TryGetHallSeatingMapById(hallSeatingMapId);
        if (hallSeatingMap == null)
        {
            return null;
        }

        return hallSeatingMap with {
            Areas = hallSeatingMap.Areas.Select(area => area with {
                Rows = ImmutableList<SeatingMapRowContract>.Empty
            }).ToImmutableList()
        };
    }
}
