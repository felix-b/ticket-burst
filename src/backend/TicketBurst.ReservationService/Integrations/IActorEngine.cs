using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations;

public interface IActorEngine : IAsyncDisposable
{
    Task StartAsync();
    Task<IEventAreaManager?> GetActor(string eventId, string areaId);
    Task ForEachLocalActor(Func<IEventAreaManager, Task> action);
}
