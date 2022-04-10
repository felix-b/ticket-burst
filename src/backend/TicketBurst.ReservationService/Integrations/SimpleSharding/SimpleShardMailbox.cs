using System.Collections.Immutable;
using System.Threading.Channels;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class SimpleShardMailbox : IAsyncDisposable
{
    private readonly int _shardCount;
    private readonly int _shardCapacity;
    private readonly EventAreaManagerInProcessCache _inprocInstanceCache;
    private readonly ImmutableList<Shard> _shards;
    private readonly CancellationTokenSource _cancellationSource = new();

    public SimpleShardMailbox(int shardCount, int shardCapacity, EventAreaManagerInProcessCache inprocInstanceCache)
    {
        _shardCount = shardCount;
        _shardCapacity = shardCapacity;
        _inprocInstanceCache = inprocInstanceCache;
        _shards = Enumerable
            .Range(0, shardCount)
            .Select(CreateShard)
            .ToImmutableList();
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationSource.Cancel();
        
        foreach (var shard in _shards)
        {
            shard.Channel.Writer.TryComplete();
        }

        await Task.WhenAll(_shards.Select(s => s.DeliveryLoopTask));
    }

    public Task<TReturn> DispatchActionAsync<TReturn>(
        string eventId,
        string areaId,
        Func<IEventAreaManager, Task<TReturn>> action)
    {
        _cancellationSource.Token.ThrowIfCancellationRequested();

        var promise = new TaskCompletionSource<TReturn>();
        Func<IEventAreaManager?, Task> body = async eam => {
            try
            {
                promise.SetResult(
                    await action(
                        eam ?? throw new Exception($"Actor not found: [{eventId}/{areaId}]")
                    )
                );
            }
            catch (Exception e)
            {
                promise.SetException(e);
            }
        };

        var recipient = new Recipient(eventId, areaId);
        var letter = new Letter(recipient, body);
        var shardIndex = GetShardIndex(recipient);

        if (!_shards[shardIndex].Channel.Writer.TryWrite(letter))
        {
            throw new Exception($"Mailbox full, cannot submit work item for [{recipient.ToString()}]");
        }

        return promise.Task;
    }

    private int GetShardIndex(Recipient recipient)
    {
        return Math.Abs(recipient.AreaId.GetHashCode()) % _shardCount;
    }

    private Shard CreateShard(int index)
    {
        var channel = Channel.CreateBounded<Letter>(_shardCapacity);
        var deliveryLoopTask = DeliveryLoop(channel, index);
        return new Shard(channel, deliveryLoopTask);
    }

    private async Task DeliveryLoop(Channel<Letter> channel, int shardIndex)
    {
        try
        {
            await foreach (var letter in channel.Reader.ReadAllAsync(_cancellationSource.Token))
            {
                if (_cancellationSource.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var actorInstance = await _inprocInstanceCache.GetActor(letter.To.EventId, letter.To.AreaId);
                    await letter.Body(actorInstance);
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"SimpleShardMailbox[{shardIndex}]: dispatched operation failed [{letter.To.EventId}/{letter.To.AreaId}]: {e.ToString()}");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine($"SimpleShardMailbox[{shardIndex}]: DeliveryLoop FAILED! {e.ToString()}");
        }
    }

    private record Shard(
        Channel<Letter> Channel,
        Task DeliveryLoopTask
    );

    private record Recipient(
        string EventId,
        string AreaId)
    {
        public override string ToString() => $"{EventId}/{AreaId}";
    }

    private record Letter(
        Recipient To,
        Func<IEventAreaManager?, Task> Body
    );
}
