using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Jobs;

public class ReservationExpiryJob : IDisposable
{
    private readonly IActorEngine _actorEngine;
    private readonly Timer _timer;

    public ReservationExpiryJob(IActorEngine actorEngine)
    {
        _actorEngine = actorEngine;
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
        _actorEngine.ForEachActor(async actor => {
            Console.WriteLine($"RESERVATION EXPIRY JOB > EAM[${actor.EventId}/${actor.AreaId}]");
            await actor.ReleaseExpiredReservations();
        }).Wait();        
    }
}
