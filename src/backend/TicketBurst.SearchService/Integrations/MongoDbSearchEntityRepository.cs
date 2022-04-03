using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;

namespace TicketBurst.SearchService.Integrations;

public class MongoDbSearchEntityRepository : ISearchEntityRepository
{
    public string MakeNewId()
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<EventContract>> GetAllEvents()
    {
        throw new NotImplementedException();
    }

    public IQueryable<EventContract> CreateEventQuery()
    {
        throw new NotImplementedException();
    }

    public Task<EventContract?> TryGetEventById(string eventId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateIsOpenForSale(string eventId, bool newValue)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<GenreContract>> GetAllGenres()
    {
        throw new NotImplementedException();
    }

    public Task<GenreContract?> TryGetGenreById(string genreId)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<ShowTypeContract>> GetAllShowTypes()
    {
        throw new NotImplementedException();
    }

    public Task<ShowTypeContract?> TryGetShowTypeById(string showTypeId)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<ShowContract>> GetAllShows()
    {
        throw new NotImplementedException();
    }

    public Task<ShowContract?> TryGetShowById(string showId)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<TroupeContract>> GetAllTroupes()
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<TroupeContract>> GetTroupesByIds(IEnumerable<string> troupeIds)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<VenueContract>> GetAllVenuesWithoutHalls()
    {
        throw new NotImplementedException();
    }

    public Task<VenueContract?> TryGetVenueById(string venueId)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<HallSeatingMapContract>> GetAllHallSeatingMapsWithoutAreas()
    {
        throw new NotImplementedException();
    }

    public Task<HallSeatingMapContract?> TryGetHallSeatingMapById(string hallSeatingMapId)
    {
        throw new NotImplementedException();
    }

    public Task<HallSeatingMapContract?> TryGetHallSeatingMapWithoutSeats(string hallSeatingMapId)
    {
        throw new NotImplementedException();
    }

    public void UpdateEventSeatingStatus(EventAreaUpdateNotificationContract notification)
    {
        throw new NotImplementedException();
    }

    public EventSeatingCacheContract RetrieveEventSeatingStatus(string eventId)
    {
        throw new NotImplementedException();
    }
}