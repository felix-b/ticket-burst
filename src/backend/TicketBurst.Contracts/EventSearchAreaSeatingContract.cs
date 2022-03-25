namespace TicketBurst.Contracts;

public record EventSearchAreaSeatingContract(
    EventSearchFullDetailContract Header,
    string HallAreaId,
    string HallAreaName,
    int AvailableCapacity,
    AreaSeatingMapContract SeatingMap
);
