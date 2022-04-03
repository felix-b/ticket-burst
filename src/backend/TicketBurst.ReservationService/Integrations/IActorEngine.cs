using TicketBurst.ReservationService.Actors;

namespace TicketBurst.ReservationService.Integrations;

public interface IActorEngine
{
    Task<EventAreaManager?> GetActor(string eventId, string areaId);
}