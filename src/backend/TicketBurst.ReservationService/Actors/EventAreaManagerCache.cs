﻿using System.Collections.Concurrent;
using System.Collections.Immutable;

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
