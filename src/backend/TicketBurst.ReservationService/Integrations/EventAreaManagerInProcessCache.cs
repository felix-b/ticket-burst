using System.Collections.Concurrent;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations;

public class EventAreaManagerInProcessCache : IActorEngine
{
    private readonly ConcurrentDictionary<string, Task<EventAreaManager?>> _loadPromiseByEventAreaKey = new();
    private readonly IReservationEntityRepository _entityRepo;

    public EventAreaManagerInProcessCache(IReservationEntityRepository entityRepo)
    {
        _entityRepo = entityRepo;
    }

    public ValueTask DisposeAsync()
    {
        // do nothing
        return ValueTask.CompletedTask; 
    }

    public Task StartAsync()
    {
        // do nothing
        return Task.CompletedTask;
    }

    public async Task<IEventAreaManager?> GetActor(string eventId, string areaId)
    {
        string key = GetEventAreaKey(eventId, areaId);

        if (!_loadPromiseByEventAreaKey.ContainsKey(key))
        {
            await AddEntry();
        }

        var promise = _loadPromiseByEventAreaKey[key];
        return await promise;

        async Task AddEntry()
        {
            var loadCompletion = new TaskCompletionSource<EventAreaManager?>();

            if (_loadPromiseByEventAreaKey.TryAdd(key, loadCompletion.Task))
            {
                Console.WriteLine($"Load EAM[{key}] > in progress.");

                try
                {
                    var actor = await LoadActor(eventId, areaId);
                    Console.WriteLine($"Load EAM[{key}] > SUCCESS");
                    loadCompletion.SetResult(actor);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Load EAM[{key}] > FAILED! {e.ToString()}");
                    loadCompletion.SetResult(null);
                }
            }
        }
    }

    public async Task ForEachLocalActor(Func<IEventAreaManager, Task> action)
    {
        var snapshot = _loadPromiseByEventAreaKey.Values.ToArray();
        
        foreach (var promise in snapshot.Where(p => p.IsCompleted))
        {
            try
            {
                var actor = promise.Result;
                if (actor != null)
                {
                    await action(actor);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }            
        }
    }

    public ClusterDiagnosticInfo GetClusterDiagnostics()
    {
        return new ClusterDiagnosticInfo(
            Address: string.Empty,
            ClusterName: string.Empty,
            ThisMember: new MemberDiagnosticInfo(string.Empty, string.Empty, string.Empty, 0),
            OtherMembers: new MemberDiagnosticInfo[0],
            LocalGrains: new string[0]
        );
    }

    private async Task<EventAreaManager?> LoadActor(string eventId, string areaId)
    {
        var actor = new EventAreaManager(eventId, areaId, _entityRepo);

        try
        {
            await actor.RecoverState();
            return actor;
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR! EAM[{eventId}/{areaId}] failed to recover! {e.ToString()}");
            return null;
        }
    }
    
    private string GetEventAreaKey(string eventId, string areaId)
    {
        return $"{eventId}/{areaId}";
    }
}
