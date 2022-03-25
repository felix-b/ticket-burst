using System.Collections.Immutable;

namespace TicketBurst.SearchService;

public record EventSeatingCacheContract(
    string EventId,
    int TotalCapacity,
    int AvailableCapacity,
    ImmutableDictionary<string, EventAreaSeatingCacheContract> SeatingByAreaId
);

public record EventAreaSeatingCacheContract(
    string HallAreaId,
    int TotalCapacity,
    int AvailableCapacity,
    ulong NotificationSequenceNo,
    ImmutableHashSet<string> AvailableSeatIds
);
