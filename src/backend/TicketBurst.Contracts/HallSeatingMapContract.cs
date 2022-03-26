using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record HallSeatingMapContract(
    string Id,
    string HallId,
    string Name,
    string PlanImageUrl,
    int Capacity,
    ImmutableList<PriceLevelContract> PriceLevels,
    ImmutableList<AreaSeatingMapContract> Areas
);

public record AreaSeatingMapContract(
    string SeatingMapId,
    string HallAreaId,
    string HallAreaName,
    string PlanImageUrl,
    int Capacity,
    ImmutableList<SeatingMapRowContract> Rows
);

public record SeatingMapRowContract(
    string Id,   
    string Name,
    ImmutableList<SeatingMapSeatContract> Seats
);

public record SeatingMapSeatContract(
    string Id,
    string Name,
    string PriceLevelId,
    SeatStatus? Status = null
);
