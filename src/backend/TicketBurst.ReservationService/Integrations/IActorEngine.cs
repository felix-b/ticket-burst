using TicketBurst.ReservationService.Actors;

namespace TicketBurst.ReservationService.Integrations;

public interface IActorEngine
{
    Task<IEventAreaManager?> GetActor(string eventId, string areaId);
    Task ForEachActor(Func<IEventAreaManager, Task> action);
}
