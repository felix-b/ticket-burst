using System.Collections.Concurrent;

namespace TicketBurst.ReservationService.Actors;

public class EventAreaManagerCache
{
    private readonly ConcurrentDictionary<string, Task<EventAreaManager?>> _loadPromiseByEventAreaKey = new();

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
                var actor = await LoadActor(eventId, areaId);
                loadCompletion.SetResult(actor);
            }
        }
    }

    private async Task<EventAreaManager?> LoadActor(string eventId, string areaId)
    {
        var actor = new EventAreaManager(eventId, areaId);

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

    public static readonly EventAreaManagerCache SingletonInstance = new();
}
