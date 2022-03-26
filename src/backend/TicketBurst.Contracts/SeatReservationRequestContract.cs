using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public class SeatReservationRequestContract
{
    public SeatReservationRequestContract(string eventId, string hallAreaId, string[] seatIds, string? clientContext)
    {
        EventId = eventId;
        HallAreaId = hallAreaId;
        SeatIds = seatIds;
        ClientContext = clientContext;
    }

    public string EventId { get; init; }
    public string HallAreaId { get; init; }
    public string[] SeatIds { get; init; }
    public string? ClientContext { get; init; }
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
