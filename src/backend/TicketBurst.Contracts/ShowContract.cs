using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record ShowContract(
    string Id, 
    string ShowTypeId,
    string GenreId,
    ImmutableList<string>? TroupeIds,
    string Title,
    string Description,
    string PosterImageUrl
);
