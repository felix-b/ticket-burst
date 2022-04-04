using System.Collections.Immutable;

namespace TicketBurst.SearchService.Contracts;

public record EventSeatingCacheContract(
    string EventId,
    int TotalCapacity,
    int AvailableCapacity,
    ImmutableDictionary<string, EventAreaSeatingCacheContract> SeatingByAreaId)
{
    public bool IsTrusted => SeatingByAreaId.Values.All(area => area.IsTrusted);
}
    
public record EventAreaSeatingCacheContract(
    string HallAreaId,
    int TotalCapacity,
    int AvailableCapacity,
    ulong NotificationSequenceNo,
    ImmutableHashSet<string> AvailableSeatIds)
{
    public bool IsTrusted => NotificationSequenceNo > 0;
}
