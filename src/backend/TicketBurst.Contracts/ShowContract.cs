namespace TicketBurst.Contracts;

public record ShowContract(
    string ShowTypeId,
    string GenreId,
    string Id, 
    string Name,
    string PosterImageUrl
);
