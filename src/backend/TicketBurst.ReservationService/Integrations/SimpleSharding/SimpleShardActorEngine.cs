using System.Threading.Channels;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class SimpleShardActorEngine : IActorEngine
{
    private readonly IClusterInfoProvider _clusterInfo;
    private readonly SimpleStatefulClusterMember _cluster;
    private readonly SimpleShardMailbox _inprocMailbox;
    private readonly EventAreaManagerInProcessCache _inprocActorCache;
    //private readonly EventAreaManagerShardClientPool _shardClientPool;
    private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

    public SimpleShardActorEngine(
        IClusterInfoProvider clusterInfo, 
        SimpleStatefulClusterMember cluster, 
        SimpleShardMailbox inprocMailbox,
        EventAreaManagerInProcessCache inprocActorCache)
        //EventAreaManagerShardClientPool shardClientPool)
    {
        _clusterInfo = clusterInfo;
        _cluster = cluster;
        _inprocMailbox = inprocMailbox;
        _inprocActorCache = inprocActorCache;
        //_shardClientPool = 

        _cluster.Changed += OnClusterChange;
    }

    public ValueTask DisposeAsync()
    {
        _cluster.Changed -= OnClusterChange;
        _cancellationSource.Cancel();
        return ValueTask.CompletedTask;
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task<IEventAreaManager?> GetActor(string eventId, string areaId)
    {
        var proxy = CreateActorProxy(eventId, areaId);
        return Task.FromResult<IEventAreaManager?>(proxy);
    }

    public async Task ForEachLocalActor(Func<IEventAreaManager, Task> action)
    {
        var localActors = new List<IEventAreaManager>();

        await _inprocActorCache.ForEachLocalActor(actor => {
            localActors.Add(actor);
            return Task.CompletedTask;
        });

        foreach (var actor in localActors)
        {
            var proxy = CreateActorProxy(actor.EventId, actor.AreaId);
            await action(proxy);
        }
    }

    public string[] GetLocalActorIds()
    {
        return _inprocActorCache.GetLocalActorIds();
    }

    private IEventAreaManager CreateActorProxy(string eventId, string areaId)
    {
        var proxy = new EventAreaManagerHttpRoutingProxy(
            eventId,
            areaId,
            _cluster,
            _inprocMailbox,
            _cancellationSource.Token);
        return proxy;
    }

    private void OnClusterChange()
    {
        Console.WriteLine("SimpleShardActorEngine> SCAVENGE after cluster change");
        
        var state = _cluster.CurrentState;

        if (state.Status == ClusterStatus.Steady)
        {
            _inprocActorCache.Scavenge(retentionPredicate: RetentionPredicate);
        }

        bool RetentionPredicate(string eventId, string areaId)
        {
            return _cluster.GetShardMemberIndex(areaId) == state.ThisMemberIndex;
        }
    }
}
