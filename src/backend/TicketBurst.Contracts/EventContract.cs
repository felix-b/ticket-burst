using System;

namespace TicketBurst.Contracts;

public record EventContract(
    string HallId,
    string HallName,
    string HallSeatingMapId,
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
    string Id,
    string Title,
    string Description,
    string PosterImageUrl,
    DateTimeOffset SaleStartTime,
    DateTimeOffset EventStartTime,
    int DurationMinutes,
    bool CanBuyTickets
);
