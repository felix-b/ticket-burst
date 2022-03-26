using System;

namespace TicketBurst.Contracts;

public record TicketContract(
    string Id,
    string EventId,
    string HallAreaId,
    string RowId,
    string SeatId,
    string PriceLevelId,
    string VenueName,
    string VenueAddress,
    string ShowTitle,
    string? EventTitle,
    string HallName,
    string AreaName,
    string RowName,
    string SeatName,
    DateTime StartLocalTime,
    int DurationMinutes,
    string PriceLevelName,
    decimal Price
);
