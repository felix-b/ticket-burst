
using System.Collections.Immutable;

namespace TicketBurst.ServiceInfra;

public interface IClusterInfoProvider : IDisposable
{
    ClusterInfo? TryGetInfo();
    
    DateTime UtcNow { get; }

    public ClusterInfo GetInfoOrThrow()
    {
        return TryGetInfo() ?? throw new Exception($"{this.GetType().Name} did not retrieve cluster info");
    }
}

public record ClusterInfo(
    int MemberCount,
    int ThisMemberIndex,
    long Generation,
    ImmutableList<string> MemberHostNames)
{
    public static readonly ClusterInfo Empty = new ClusterInfo(
        MemberCount: 0,
        MemberHostNames: ImmutableList<string>.Empty,
        ThisMemberIndex: -1,
        Generation: 0);

    public static readonly ClusterInfo DevboxDefault = new ClusterInfo(
        MemberCount: 1,
        MemberHostNames: ImmutableList<string>.Empty.Add($"localhost:8090"),
        ThisMemberIndex: 0,
        Generation: 1);
}

public record SimpleStatefulClusterState(
    int MemberCount,
    ImmutableList<string> MemberHostNames,
    int ThisMemberIndex,
    long Generation,
    ClusterStatus Status,
    DateTime SinceUtc,
    int? PendingRebalanceMemberCount = null)
{
    public TimeSpan GetStatusDuration() => DateTime.UtcNow.Subtract(SinceUtc);
    public TimeSpan GetStatusDuration(DateTime utcNow) => utcNow.Subtract(SinceUtc);
}

public enum ClusterStatus
{
    Steady = 0,
    WillRebalance = 1,
    RebalanceLockdown = 2
}
