using System.Collections.Immutable;

#pragma disable CS0067

namespace TicketBurst.ServiceInfra;

public class DevboxClusterInfoProvider : IClusterInfoProvider
{
    private ClusterInfo _info = ClusterInfo.DevboxDefault;
    
    public DevboxClusterInfoProvider()
    {
    }

    public DevboxClusterInfoProvider(int[] portNumbers, int thisPortNumber)
    {
        _info = new ClusterInfo(
            MemberCount: portNumbers.Length,
            ThisMemberIndex: Array.IndexOf(portNumbers, thisPortNumber),
            Generation: 1,
            MemberHostNames: portNumbers.Select(p => $"localhost:{p}").ToImmutableList()
        );
    }

    public void Dispose()
    {
        // do nothing
    }

    public ClusterInfo? TryGetInfo() => _info;
    public DateTime UtcNow => DateTime.UtcNow;
}
