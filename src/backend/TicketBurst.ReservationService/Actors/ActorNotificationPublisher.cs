using System.Threading.Channels;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;
using Timer = System.Threading.Timer;

namespace TicketBurst.ReservationService.Actors;

public class ActorNotificationPublisher : IDisposable
{
    private readonly EventAreaManagerCache _actorCache;
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
    private readonly Channel<EventAreaUpdateNotificationContract> _sendQueue;
    private readonly Task _senderTask;

    public ActorNotificationPublisher(EventAreaManagerCache actorCache)
    {
        _actorCache = actorCache;
        _timer = new Timer(HandleTimerTick, state: null, dueTime: TimeSpan.FromSeconds(15), period: TimeSpan.FromSeconds(15));
        
        _sendQueue = Channel.CreateBounded<EventAreaUpdateNotificationContract>(
            options: new BoundedChannelOptions(capacity: 1000),
            itemDropped: (item) => {
                Console.WriteLine($"ERROR! EAM CAPACITY NOTIFICATION DROPPED! [{item.EventId}/{item.HallAreaId}#{item.SequenceNo}]");
            });
        
        _senderTask = RunSenderLoop();
    }

    public void Dispose()
    {
        _timer.Dispose();
        _cancellation.Cancel();
        
        if (!_senderTask.Wait(TimeSpan.FromSeconds(10)))
        {
            Console.WriteLine($"PUB/SUB: SENDER LOOP FAILED TO STOP IN TIMELY FASHION");
        }
    }

    private void HandleTimerTick(object? state)
    {
        _actorCache.ForEachActor(actor => {
            var notification = actor.GetUpdateNotification();
            if (!_sendQueue.Writer.TryWrite(notification))
            {
                Console.WriteLine(
                    $"PUB/SUB: EAM[{notification.EventId}/{notification.HallAreaId}#{notification.SequenceNo}] FAILED TO ENQUEUE!");
            }
        });        
    }

    private async Task RunSenderLoop()
    {
        Console.WriteLine($"PUB/SUB: SENDER LOOP STARTING");

        try
        {
            await foreach (var message in _sendQueue.Reader.ReadAllAsync(_cancellation.Token))
            {
                Console.WriteLine(
                    $"PUB/SUB: EAM[{message.EventId}/{message.HallAreaId}#{message.SequenceNo}] sending capacity notification");

                try
                {
                    var reply = await ServiceClient.HttpPostJson<string>(
                        ServiceName.Search,
                        path: new[] { "capacity", "update" },
                        body: message);

                    Console.WriteLine(
                        $"PUB/SUB: EAM[{message.EventId}/{message.HallAreaId}#{message.SequenceNo}] notification sent, reply=[{reply}]");
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"PUB/SUB: EAM[{message.EventId}/{message.HallAreaId}#{message.SequenceNo}] notification send FAILED! {e.ToString()}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"PUB/SUB: SENDER LOOP STOP REQUESTED");
        }
        catch (Exception e)
        {
            Console.WriteLine($"PUB/SUB: SENDER LOOP CRASHED! {e.ToString()}");
            throw;
        }

        Console.WriteLine($"PUB/SUB: SENDER LOOP STOPPED");
    }
}