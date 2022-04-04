using System.Collections.Concurrent;

namespace TicketBurst.ServiceInfra;

public class ReadThroughCache<TKey, TValue>
    where TKey : notnull
{
    private readonly string _cacheName;
    private readonly Func<TKey, Task<TValue?>> _loadValue;
    private readonly ConcurrentDictionary<TKey, Task<TValue?>> _valuePromiseByKey = new();

    public ReadThroughCache(string cacheName, Func<TKey, Task<TValue?>> loadValue)
    {
        _cacheName = cacheName;
        _loadValue = loadValue;
    }

    public async Task<TValue?> Get(TKey key)
    {
        if (!_valuePromiseByKey.ContainsKey(key))
        {
            await AddEntry();
        }

        var promise = _valuePromiseByKey[key];
        return await promise;

        async Task AddEntry()
        {
            var loadCompletion = new TaskCompletionSource<TValue?>();

            if (_valuePromiseByKey.TryAdd(key, loadCompletion.Task))
            {
                Console.WriteLine($"READ-THROUGH-CACHE[{_cacheName}]: miss [{key}], loading value.");

                try
                {
                    var value = await _loadValue(key);
                    Console.WriteLine($"READ-THROUGH-CACHE[{_cacheName}]: load value for key [{key}], success");
                    loadCompletion.SetResult(value);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"READ-THROUGH-CACHE[{_cacheName}]: load value for key [{key}], FAILURE! {e.ToString()}");
                    loadCompletion.SetResult(default);
                }
            }
        }
    }

    public void Set(TKey key, TValue? value)
    {
        _valuePromiseByKey[key] = Task.FromResult(value);
    }

    public IEnumerable<TValue> TakeSnapshotOfValues()
    {
        var promisesArray = _valuePromiseByKey.Values.ToArray();
        return promisesArray
            .Where(p => p.IsCompleted && p.Result != null)
            .Select(p => p.Result!);
    }
}