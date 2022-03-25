using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventSearchFullDetailContract(
    EventSearchResultContract Event,
    EventSearchFullDetailContract.HallInfo Hall)
{
    public record HallInfo(
        string SeatingPlanImageUrl,
        int TotalCapacity,
        int AvailableCapacity,
        ImmutableList<AreaInfo> Areas
    );

    public record AreaInfo(
        string HallAreaId,   
        string Name,
        string SeatingPlanImageUrl,
        int TotalCapacity,
        int AvailableCapacity
    );
}
