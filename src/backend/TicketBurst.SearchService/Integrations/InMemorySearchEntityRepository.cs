#pragma warning disable CS1998

using System.Collections.Immutable;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;
using TicketBurst.SearchService.Logic;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Integrations;

public class InMemorySearchEntityRepository : ISearchEntityRepository
{
    public string MakeNewId()
    {
        return Guid.NewGuid().ToString("d");
    }

    public async Task<IAsyncEnumerable<EventContract>> GetAllEvents()
    {
        return MockDatabase.Events.All.ToAsyncEnumerable();
    }

    public IQueryable<EventContract> CreateEventQuery()
    {
        return MockDatabase.Events.All.AsQueryable();
    }

    public async Task<EventContract?> TryGetEventById(string eventId)
    {
        return MockDatabase.Events.All.FirstOrDefault(e => e.Id == eventId);
    }

    public Task UpdateIsOpenForSale(string eventId, bool newValue)
    {
        MockDatabase.Events.UpdateIsOpenForSale(eventId, newValue);
        return Task.CompletedTask;
    }

    public async Task<IAsyncEnumerable<GenreContract>> GetAllGenres()
    {
        return MockDatabase.Genres.All.ToAsyncEnumerable();
    }

    public async Task<GenreContract?> TryGetGenreById(string genreId)
    {
        return MockDatabase.Genres.All.FirstOrDefault(g => g.Id == genreId);
    }

    public async Task<IAsyncEnumerable<ShowTypeContract>> GetAllShowTypes()
    {
        return MockDatabase.ShowTypes.All.ToAsyncEnumerable();
    }

    public async Task<ShowTypeContract?> TryGetShowTypeById(string showTypeId)
    {
        return MockDatabase.ShowTypes.All.FirstOrDefault(s => s.Id == showTypeId);
    }

    public async Task<IAsyncEnumerable<ShowContract>> GetAllShows()
    {
        return MockDatabase.Shows.All.ToAsyncEnumerable();
    }

    public async Task<ShowContract?> TryGetShowById(string showId)
    {
        return MockDatabase.Shows.All.FirstOrDefault(s => s.Id == showId);
    }

    public async Task<IAsyncEnumerable<TroupeContract>> GetAllTroupes()
    {
        return MockDatabase.Troupes.All.ToAsyncEnumerable();
    }

    public async Task<IAsyncEnumerable<TroupeContract>> GetTroupesByIds(IEnumerable<string> troupeIds)
    {
        return MockDatabase.Troupes.All
            .Where(t => troupeIds.Contains(t.Id))
            .ToAsyncEnumerable();
    }

    public async Task<IAsyncEnumerable<VenueContract>> GetAllVenuesWithoutHalls()
    {
        return MockDatabase.Venues.All
            .Select(v => v with { Halls = ImmutableList<HallContract>.Empty})
            .ToAsyncEnumerable();
    }

    public async Task<VenueContract?> TryGetVenueById(string venueId)
    {
        return MockDatabase.Venues.All.FirstOrDefault(v => v.Id == venueId);
    }

    public async Task<IAsyncEnumerable<HallSeatingMapContract>> GetAllHallSeatingMapsWithoutAreas()
    {
        return MockDatabase.HallSeatingMaps.All
            .Select(m => m with { Areas = ImmutableList<AreaSeatingMapContract>.Empty })
            .ToAsyncEnumerable();
    }

    public async Task<HallSeatingMapContract?> TryGetHallSeatingMapById(string hallSeatingMapId)
    {
        return MockDatabase.HallSeatingMaps.All.FirstOrDefault(m => m.Id == hallSeatingMapId);
    }

    public async Task<HallSeatingMapContract?> TryGetHallSeatingMapWithoutSeats(string hallSeatingMapId)
    {
        var hallSeatingMap = MockDatabase.HallSeatingMaps.All.FirstOrDefault(m => m.Id == hallSeatingMapId);
        if (hallSeatingMap == null)
            return null;

        return hallSeatingMap with {
            Areas = hallSeatingMap.Areas.Select(area => area with {
                Rows = ImmutableList<SeatingMapRowContract>.Empty
            }).ToImmutableList()
        };
    }
}
