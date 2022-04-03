using TicketBurst.Contracts;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;
using Timer = System.Threading.Timer;

namespace TicketBurst.ReservationService.Jobs;

public class EventAreaUpdateNotificationJob : IDisposable
{
    private readonly EventAreaManagerInProcessCache _actorCache;
    private readonly IMessagePublisher<EventAreaUpdateNotificationContract> _publisher;
    private readonly Timer _timer;

    public EventAreaUpdateNotificationJob(
        EventAreaManagerInProcessCache actorCache,
        IMessagePublisher<EventAreaUpdateNotificationContract> publisher)
    {
        _actorCache = actorCache;
        _publisher = publisher;
        _timer = new Timer(
            InProcessJob.WithTimerErrorHandling(this, HandleTimerTick), 
            state: null, 
            dueTime: TimeSpan.FromSeconds(15), 
            period: TimeSpan.FromSeconds(15));
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private void HandleTimerTick()
    {
        _actorCache.ForEachActor(actor => {
            var notification = actor.GetUpdateNotification();
            _publisher.Publish(notification);
        });
    }
}
