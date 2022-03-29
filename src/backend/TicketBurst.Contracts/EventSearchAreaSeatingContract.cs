using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventSearchAreaSeatingContract(
    EventSearchFullDetailContract Header,
    string HallAreaId,
    string HallAreaName,
    int AvailableCapacity,
    ImmutableList<PriceLevelContract> PriceLevels,
    AreaSeatingMapContract SeatingMap
);
