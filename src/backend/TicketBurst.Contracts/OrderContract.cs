using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record OrderContract(
    uint OrderNumber,
    OrderStatus Status,
    string OrderDescription,
    DateTime CreatedAtUtc,
    string CustomerName,
    string CustomerEmail,
    ImmutableList<TicketContract> Tickets,
    string PaymentCurrency,
    decimal PaymentSubtotal,
    decimal PaymentTax,
    decimal PaymentTotal,
    string PaymentToken,
    string ReservationId,
    DateTime? PaymentReceivedUtc = null,
    DateTime? TicketsShippedUtc = null
);

public enum OrderStatus
{
    Preview = 0,
    CompletionInProgress = 1,
    Completed = 2,
    FailedToComplete = 3,
    RefundInProgress = 4,
    RefundCompleted = 5,
    RefundFailed = 6,
    RefundFailureHandled = 7
}
