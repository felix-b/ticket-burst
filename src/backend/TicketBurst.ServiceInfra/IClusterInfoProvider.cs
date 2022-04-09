using System.Collections.Immutable;

namespace TicketBurst.ServiceInfra;

public interface IClusterInfoProvider
{
    ClusterInfo Current { get; }
    event Action Changed;
}

public record ClusterInfo(
    int MemberCount,
    ImmutableList<string> MemberHostNames,
    int ThisMemberIndex,
    long Generation)
{
    public static readonly ClusterInfo Empty = new ClusterInfo(
        MemberCount: 0,
        MemberHostNames: ImmutableList<string>.Empty,
        ThisMemberIndex: -1,
        Generation: 0);

    public static readonly ClusterInfo DevBox = new ClusterInfo(
        MemberCount: 1,
        MemberHostNames: ImmutableList<string>.Empty.Add(Environment.MachineName),
        ThisMemberIndex: 0,
        Generation: 1);
}
