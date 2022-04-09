#pragma warning disable CS0067

using System.Collections.Immutable;

namespace TicketBurst.ServiceInfra;

public class DevboxClusterInfoProvider : IClusterInfoProvider
{
    private readonly int _thisPortNumber;
    private ClusterInfo _info;
    private System.Threading.Timer? _timer = null;
    
    public DevboxClusterInfoProvider(int thisPortNumber)
    {
        _thisPortNumber = thisPortNumber;
        _info = MakeClusterInfo(new[] { thisPortNumber }, thisPortNumber, generation: 1);
    }

    public DevboxClusterInfoProvider(int[] portNumbers, int thisPortNumber)
    {
        _thisPortNumber = thisPortNumber;
        _info = MakeClusterInfo(portNumbers, thisPortNumber, generation: 1);
    }

    public void InjectChange(int[] portNumbers)
    {
        _info = MakeClusterInfo(portNumbers, _thisPortNumber, generation: _info.Generation + 1);
    }

    public void StartPollingForChanges()
    {
        _timer = new Timer(CheckForChanges, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

        void CheckForChanges(object? state)
        {
            try
            {
                var lines = File.ReadAllLines(DevboxClusterOrchestrator.MemberListFilePath);
                var newPortNumbers = lines
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(Int32.Parse)
                    .ToArray();
                InjectChange(newPortNumbers);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public ClusterInfo? TryGetInfo() => _info;
    public DateTime UtcNow => DateTime.UtcNow;

    private static ClusterInfo MakeClusterInfo(int[] portNumbers, int thisPortNumber, long generation)
    {
        return new ClusterInfo(
            MemberCount: portNumbers.Length,
            ThisMemberIndex: Array.IndexOf(portNumbers, thisPortNumber),
            Generation: generation,
            MemberHostNames: portNumbers.Select(p => $"localhost:{p}").ToImmutableList()
        );
    }
}
