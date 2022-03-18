namespace TicketBurst.Contracts;

public record RowContract(
    string Id,   
    string Name,
    SeatContract[]? Seats
);
