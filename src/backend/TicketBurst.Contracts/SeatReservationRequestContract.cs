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
    string? ReservationId,
    DateTime? ReservationExpiryUtc,
    string? ErrorCode
);
