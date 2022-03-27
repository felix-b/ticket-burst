using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventAreaUpdateNotificationContract(
    string Id,
    ulong SequenceNo,
    DateTime PublishedAtUtc,
    string EventId,
    string HallAreaId,
    int TotalCapacity,
    int AvailableCapacity,
    ImmutableDictionary<string, SeatStatus> StatusBySeatId)
{
    public override string ToString() => $"{EventId}/{HallAreaId}#{SequenceNo}";
}

public enum SeatStatus
{
    Unspecified = 0,
    Available = 1,
    Reserved = 2,
    Sold = 3
}
