﻿using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventSearchResultContract(
    string EventId,
    string HallId,
    string HallName,
    string VenueId,
    string VenueName,
    string VenueAddress,
    GeoPointContract VenueLocation,
    TimeZoneContract VenueTimeZone,
    string ShowId,
    string ShowName,
    string ShowDescription,
    string ShowTypeId,
    string ShowTypeName,
    string GenreId,
    string GenreName,
    string EventTitle,
    string? EventDescription,
    string PosterImageUrl,
    ImmutableList<string>? TroupeIds,
    DateTimeOffset SaleStartTime,
    DateTimeOffset EventStartTime,
    int DurationMinutes,
    bool CanBuyTickets,
    int NumberOfSeatsLeft
);
