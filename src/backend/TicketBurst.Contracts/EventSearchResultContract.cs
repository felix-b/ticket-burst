using System;

namespace TicketBurst.Contracts;

public record EventSearchResultContract(
    string EventId,
    string HallId,
    string HallName,
    string VenueId,
    string VenueName,
    string VenueAddress,
    double VenueLocationLat,
    double VenueLocationLon,
    string ShowId,
    string ShowName,
    string ShowTypeId,
    string ShowTypeName,
    string GenreId,
    string GenreName,
    string Title,
    DateTimeOffset SaleStartTime,
    DateTimeOffset EventStartTime,
    int DurationMinutes,
    bool CanBuyTickets
);
