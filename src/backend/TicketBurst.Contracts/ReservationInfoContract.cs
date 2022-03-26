using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record ReservationInfoContract(
    string Id,
    string EventId,
    string HallAreaId,
    ImmutableList<string> SeatIds
);
