using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventSaleNotificationContract(
    string Id,
    DateTime PublishedAtUtc,
    string EventId,
    ImmutableList<string> HallAreaIds,
    DateTime SaleStartUtc)
{
    public override string ToString() => $"{EventId}:${SaleStartUtc.ToString("yyyyMMdd-HHmmss")}";
}
