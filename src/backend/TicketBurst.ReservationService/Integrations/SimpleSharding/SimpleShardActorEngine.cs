using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class SimpleShardActorEngine : IActorEngine
{
    private readonly IClusterInfoProvider _clusterInfo;

    public SimpleShardActorEngine(IClusterInfoProvider clusterInfo)
    {
        _clusterInfo = clusterInfo;
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public Task StartAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEventAreaManager?> GetActor(string eventId, string areaId)
    {
        throw new NotImplementedException();
    }

    public Task ForEachLocalActor(Func<IEventAreaManager, Task> action)
    {
        throw new NotImplementedException();
    }

    public ClusterDiagnosticInfo GetClusterDiagnostics()
    {
        throw new NotImplementedException();
    }
}
