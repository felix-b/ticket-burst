using System.Collections.Concurrent;
using TicketBurst.ReservationService.Actors;

namespace TicketBurst.ReservationService.Integrations;

public class EventAreaManagerInProcessCache : IActorEngine
{
    private readonly ConcurrentDictionary<string, Task<EventAreaManager?>> _loadPromiseByEventAreaKey = new();
    private readonly IReservationEntityRepository _entityRepo;

    public EventAreaManagerInProcessCache(IReservationEntityRepository entityRepo)
    {
        _entityRepo = entityRepo;
    }

    public async Task<EventAreaManager?> GetActor(string eventId, string areaId)
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

    public void ForEachActor(Action<EventAreaManager> action)
    {
        var snapshot = _loadPromiseByEventAreaKey.Values.ToArray();
        
        foreach (var promise in snapshot.Where(p => p.IsCompleted))
        {
            try
            {
                var actor = promise.Result;
                if (actor != null)
                {
                    action(actor);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }            
        }
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
