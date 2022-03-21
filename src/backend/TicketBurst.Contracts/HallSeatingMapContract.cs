using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record HallSeatingMapContract(
    string Id,
    string HallId,
    string Name,
    ImmutableList<AreaSeatingMapContract> Areas
);

public record AreaSeatingMapContract(
    string Id,
    string HallAreaId,
    ImmutableList<SeatingMapRowContract> Rows
);

public record SeatingMapRowContract(
    string Id,   
    string Name,
    ImmutableList<SeatingMapSeatContract> Seats
);

public record SeatingMapSeatContract(
    string Id,
    string Name
);
