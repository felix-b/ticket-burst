using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Jobs;

public class ReservationExpiryJob : IDisposable
{
    private readonly EventAreaManagerInProcessCache _actorCache;
    private readonly Timer _timer;

    public ReservationExpiryJob(EventAreaManagerInProcessCache actorCache)
    {
        _actorCache = actorCache;
        _timer = new Timer(
            InProcessJob.WithTimerErrorHandling(this, HandleTimerTick), 
            state: null, 
            dueTime: TimeSpan.FromSeconds(20), 
            period: TimeSpan.FromSeconds(20));
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private void HandleTimerTick()
    {
        _actorCache.ForEachActor(actor => {
            Console.WriteLine($"RESERVATION EXPIRY JOB > EAM[${actor.EventId}/${actor.AreaId}]");
            actor.ReleaseExpiredReservations();
        });        
    }
}
