using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record HallContract(
    string Id,   
    string Name,
    string SeatingPlanImageUrl, 
    ImmutableList<HallAreaContract> Areas,
    string DefaultSeatingMapId
);
