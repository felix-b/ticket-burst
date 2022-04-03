using System.Runtime.CompilerServices;
using MongoDB.Driver;

namespace TicketBurst.ServiceInfra;

public static class AsyncExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
        this IAsyncCursor<T> cursor, 
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        while (await cursor.MoveNextAsync(cancellation))
        {
            var batch = cursor.Current;
            if (batch != null)
            {
                foreach (var item in batch)
                {
                    yield return item;
                }
            }
        }
    }

    public static IList<T> ToListSync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var results = new List<T>();
        FetchAsync().Wait();
        return results;
        
        async Task FetchAsync()
        {
            await foreach (var item in asyncEnumerable)
            {
                results.Add(item);
            }
        }
    }

    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> syncEnumerable)
    {
        return new AsyncEnumerableAdapter<T>(syncEnumerable);
    }

    private class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _syncEnumerable;

        public AsyncEnumerableAdapter(IEnumerable<T> syncEnumerable)
        {
            _syncEnumerable = syncEnumerable;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new AsyncEnumerator<T>(_syncEnumerable.GetEnumerator(), cancellationToken);
        }
    }

    private class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _syncEnumerator;
        private readonly CancellationToken _cancellationToken;

        public AsyncEnumerator(IEnumerator<T> syncEnumerator, CancellationToken cancellationToken)
        {
            _syncEnumerator = syncEnumerator;
            _cancellationToken = cancellationToken;
        }

        public ValueTask DisposeAsync()
        {
            _syncEnumerator.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            var result = _syncEnumerator.MoveNext();
            return new ValueTask<bool>(result);
        }

        public T Current => _syncEnumerator.Current;
    }
}