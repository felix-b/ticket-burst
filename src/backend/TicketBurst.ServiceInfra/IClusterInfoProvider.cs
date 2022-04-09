
using System.Collections.Immutable;

namespace TicketBurst.ServiceInfra;

public interface IClusterInfoProvider : IDisposable
{
    ClusterInfo Current { get; }
    event Action Changed;
}

public record ClusterInfo(
    int MemberCount,
    ImmutableList<string> MemberHostNames,
    int ThisMemberIndex,
    long Generation,
    ClusterStatus Status,
    DateTime SinceUtc,
    int? PendingRebalanceMemberCount = null)
{

    public TimeSpan StatusDuration => DateTime.UtcNow.Subtract(SinceUtc);
        
    public static readonly ClusterInfo Empty = new ClusterInfo(
        MemberCount: 0,
        MemberHostNames: ImmutableList<string>.Empty,
        ThisMemberIndex: -1,
        Generation: 0,
        Status: ClusterStatus.Steady,
        SinceUtc: DateTime.UtcNow);

    public static readonly ClusterInfo DevBox = new ClusterInfo(
        MemberCount: 1,
        MemberHostNames: ImmutableList<string>.Empty.Add(Environment.MachineName),
        ThisMemberIndex: 0,
        Generation: 1,
        Status: ClusterStatus.Steady,
        SinceUtc: DateTime.UtcNow);
}

public enum ClusterStatus
{
    Steady = 0,
    WillRebalance = 1,
    RebalanceLockdown = 2
}
