using System.Collections.Immutable;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Jobs;

public class EventWarmupJob : InProcessJob<EventWarmupJob.WorkItem>
{
    public EventWarmupJob(EventAreaManagerInProcessCache cache) 
        : base(nameof(EventWarmupJob), boundCapacity: 1000, CreateDoWork(cache))
    {
    }

    public void Enqueue(string eventId, ImmutableList<string> areaIds)
    {
        EnqueueWorkItem(new WorkItem(eventId, areaIds));
    }

    private static Func<WorkItem, Task> CreateDoWork(EventAreaManagerInProcessCache cache)
    {
        return async workItem => {
            foreach (var areaId in workItem.AreaIds)
            {
                await cache.GetActor(workItem.EventId, areaId);
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
