using System.Collections.Immutable;

namespace TicketBurst.ReservationService;

public record ReservationJournalRecord(
    string Id,
    DateTime CreatedAtUtc,
    string EventId,
    string HallAreaId,
    string HallSeatingMapId,
    ulong SequenceNo,
    ImmutableList<string> SeatIds,
    ReservationActionType Action
);

public enum ReservationActionType
{
    Reserved = 1,
    ReleasedOrderCancelled = 2,
    ReleasedTimedOut = 3,
}
