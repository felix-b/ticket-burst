using System.Collections.Immutable;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Logic;

public class EventSeatingStatusCache
{
    private readonly object _updateSyncRoot = new();

    private readonly ISearchEntityRepository _entityRepo;
    private ImmutableDictionary<string, EventSeatingCacheContract> _byEventId = 
        ImmutableDictionary<string, EventSeatingCacheContract>.Empty;

    public ImmutableDictionary<string, EventSeatingCacheContract> ByEventId => _byEventId;

    public EventSeatingStatusCache(ISearchEntityRepository entityRepo)
    {
        _entityRepo = entityRepo;
    }

    public void Update(EventAreaUpdateNotificationContract notification)
    {
        lock (_updateSyncRoot)
        {
            if (!_byEventId.TryGetValue(notification.EventId, out var oldEventEntry))
            {
                oldEventEntry = CreateNewEventEntry(notification.EventId);
            }

            var oldAreaEntry = oldEventEntry.SeatingByAreaId[notification.HallAreaId];
            if (oldAreaEntry.NotificationSequenceNo >= notification.SequenceNo)
            {
                return; // ignore notification if already received a more recent one
            }

            var newAreaEntry = oldAreaEntry with {
                AvailableCapacity = notification.AvailableCapacity,
                AvailableSeatIds = notification.StatusBySeatId
                    .Where(kvp => kvp.Value == SeatStatus.Available)
                    .Select(kvp => kvp.Key)
                    .ToImmutableHashSet()
            };

            var deltaCapacity = newAreaEntry.AvailableCapacity - oldAreaEntry.AvailableCapacity; 
            var newEventEntry = oldEventEntry with {
                SeatingByAreaId = oldEventEntry.SeatingByAreaId.SetItem(notification.HallAreaId, newAreaEntry),
                AvailableCapacity = oldEventEntry.AvailableCapacity + deltaCapacity,
            };

            _byEventId = _byEventId.SetItem(notification.EventId, newEventEntry);
        }
    }

    public EventSeatingCacheContract Retrieve(string eventId)
    {
        if (_byEventId.TryGetValue(eventId, out var existingEntry))
        {
            return existingEntry;
        }

        lock (_updateSyncRoot)
        {
            if (_byEventId.TryGetValue(eventId, out existingEntry))
            {
                return existingEntry;
            }

            var newEntry = CreateNewEventEntry(eventId);
            _byEventId = _byEventId.SetItem(eventId, newEntry);

            return newEntry;
        }
    }

    public async Task EnsureEventAreaTrusted(string eventId, string hallAreaId)
    {
        var cachedEvent = Retrieve(eventId);
        
        if (cachedEvent.SeatingByAreaId.TryGetValue(hallAreaId, out var cachedArea))
        {
            var notification = await ServiceClient.HttpGetJson<EventAreaUpdateNotificationContract>(
                ServiceName.Reservation,
                path: new[] { "reservation", "pull-update", eventId, hallAreaId });

            if (notification != null)
            {
                Update(notification);
            }
        }
    }

    private EventSeatingCacheContract CreateNewEventEntry(string eventId)
    {
        var @event = _entityRepo.GetEventByIdOrThrowSync(eventId);
        var seatingMap = _entityRepo.GetHallSeatingMapByIdOrThrowSync(@event.HallSeatingMapId);

        var areaEntries = seatingMap.Areas
            .Select(area => new EventAreaSeatingCacheContract(
                HallAreaId: area.HallAreaId,
                TotalCapacity: area.Capacity,
                AvailableCapacity: area.Capacity,
                NotificationSequenceNo: 0,
                AvailableSeatIds: GetAllSeatIdsInArea(area)
            ));
        
        return new EventSeatingCacheContract(
            EventId: eventId,
            TotalCapacity: seatingMap.Capacity,
            AvailableCapacity: seatingMap.Capacity,
            SeatingByAreaId: areaEntries.ToImmutableDictionary(a => a.HallAreaId));

        ImmutableHashSet<string> GetAllSeatIdsInArea(AreaSeatingMapContract area)
        {
            var builder = ImmutableHashSet.CreateBuilder<string>();
            foreach (var row in area.Rows)
            {
                foreach (var seat in row.Seats)
                {
                    builder.Add(seat.Id);
                }
            }
            return builder.ToImmutable();
        }
    }
}
