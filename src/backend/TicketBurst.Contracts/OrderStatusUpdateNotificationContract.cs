using System;

namespace TicketBurst.Contracts;

public record OrderStatusUpdateNotificationContract(
    string Id,
    DateTime CreatedAtUtc,
    OrderContract UpdatedOrder)
{
    public override string ToString() => $"{UpdatedOrder.OrderNumber}:{UpdatedOrder.Status}";
}
