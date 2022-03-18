namespace TicketBurst.Contracts;

public record HallContract(
    string Id,   
    string Name,
    HallAreaContract[] Areas
);
