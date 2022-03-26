namespace TicketBurst.Contracts;

public record EventSearchAreaSeatingContract(
    EventSearchFullDetailContract Header,
    string HallAreaId,
    string HallAreaName,
    int AvailableCapacity,
    EventPriceListContract PriceList,
    AreaSeatingMapContract SeatingMap
);
