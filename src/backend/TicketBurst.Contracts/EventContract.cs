using System;
using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventContract(
    string Id,
    string VenueId,
    string HallId,
    string HallSeatingMapId,
    string ShowTypeId,
    string ShowId,
    DateTime SaleStartUtc,
    DateTime EventStartUtc,
    bool IsOpenForSale,
    int DurationMinutes,
    string? Title = null,
    string? Description = null,
    string? PosterImageUrl = null,
    ImmutableList<string>? TroupeIds = null
);
