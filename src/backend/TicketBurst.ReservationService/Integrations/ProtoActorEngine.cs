using Proto;
using TicketBurst.ReservationService.Actors;

namespace TicketBurst.ReservationService.Integrations;

public class ProtoActorEngine : IActorEngine
{
    private readonly ActorSystem _actorSystem;

    public ProtoActorEngine()
    {
        _actorSystem = new ActorSystem();
    }

    public Task<IEventAreaManager?> GetActor(string eventId, string areaId)
    {
        throw new NotImplementedException();
    }

    public Task ForEachActor(Func<IEventAreaManager, Task> action)
    {
        throw new NotImplementedException();
    }
}
