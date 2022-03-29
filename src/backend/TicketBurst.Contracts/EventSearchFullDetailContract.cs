using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventSearchFullDetailContract(
    EventSearchResultContract Event,
    EventSearchFullDetailContract.HallInfo Hall,
    EventPriceListContract PriceList)
{
    public record HallInfo(
        string SeatingPlanImageUrl,
        int TotalCapacity,
        int AvailableCapacity,
        ImmutableList<AreaInfo> Areas,
        ImmutableList<PriceLevelContract> PriceLevels
    );

    public record AreaInfo(
        string HallAreaId,   
        string Name,
        string SeatingPlanImageUrl,
        int TotalCapacity,
        int AvailableCapacity,
        decimal MinPrice,
        decimal MaxPrice
    );
}
