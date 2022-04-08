using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record ReservationJournalRecord(
    string Id,
    DateTime CreatedAtUtc,
    string EventId,
    string HallAreaId,
    string HallSeatingMapId,
    ulong SequenceNo,
    ImmutableList<string> SeatIds,
    ReservationAction Action,
    SeatStatus ResultStatus,
    uint? OrderNumber = null
);

public enum ReservationAction
{
    Unspecified = 0,
    TemporarilyReserve = 1,
    PermanentlyReservePerOrderCompleted = 2,
    ReleasePerTimeout = 3,
    ReleasePerOrderCanceled = 4,
}
