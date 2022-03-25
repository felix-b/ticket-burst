using System;

namespace TicketBurst.Contracts;

public class SeatReservationRequestContract
{
    public string EventId { get; set; }
    public string HallAreaId { get; set; }
    public string[] SeatIds { get; set; }
    public string? ClientContext { get; set; }
}

public record SeatReservationReplyContract(
    SeatReservationRequestContract Request,
    bool Success,
    string? CheckoutToken,
    DateTime? ReservationExpiryUtc,
    string? ErrorCode = null)
{
    public static SeatReservationReplyContract FromError(SeatReservationRequestContract request, string errorCode)
    {
        return new SeatReservationReplyContract(
            Request: request,
            Success: false,
            CheckoutToken: null,
            ReservationExpiryUtc: null,
            ErrorCode: errorCode);
    }
}
