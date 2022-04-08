using System.Collections.Immutable;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Jobs;

public class EventWarmupJob : InProcessJob<EventWarmupJob.WorkItem>
{
    public EventWarmupJob(IActorEngine actorEngine) 
        : base(nameof(EventWarmupJob), boundCapacity: 1000, CreateDoWork(actorEngine))
    {
    }

    public void Enqueue(string eventId, ImmutableList<string> areaIds)
    {
        EnqueueWorkItem(new WorkItem(eventId, areaIds));
    }

    private static Func<WorkItem, Task> CreateDoWork(IActorEngine actorEngine)
    {
        return async workItem => {
            foreach (var areaId in workItem.AreaIds)
            {
                var actor = await actorEngine.GetActor(workItem.EventId, areaId);
                if (actor != null)
                {
                    await actor.Ping();
                }
            }
        };
    }

    public record WorkItem(
        string EventId,
        ImmutableList<string> AreaIds)
    {
        public override string ToString() => $"{AreaIds.Count}xEvent[{EventId}]";
    }
}
