using TicketBurst.ReservationService.Actors;

namespace TicketBurst.ReservationService.Jobs;

public class ReservationExpiryJob : IDisposable
{
    private readonly EventAreaManagerCache _actorCache;
    private readonly Timer _timer;

    public ReservationExpiryJob(EventAreaManagerCache actorCache)
    {
        _actorCache = actorCache;
        _timer = new Timer(HandleTimerTick, state: null, dueTime: TimeSpan.FromSeconds(20), period: TimeSpan.FromSeconds(20));
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private void HandleTimerTick(object? state)
    {
        _actorCache.ForEachActor(actor => {
            Console.WriteLine($"RESERVATION EXPIRY JOB > EAM[${actor.EventId}/${actor.AreaId}]");
            actor.ReleaseExpiredReservations();
        });        
    }
}
