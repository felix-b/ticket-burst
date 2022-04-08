using TicketBurst.Contracts;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Actors;

public interface IEventAreaManager
{
    Task<SeatReservationReplyContract> TryReserveSeats(SeatReservationRequestContract request);
    Task ReleaseExpiredReservations();
    EventAreaUpdateNotificationContract GetUpdateNotification();
    ReservationJournalRecord? FindEffectiveJournalRecordById(string reservationId);
    Task<bool> UpdateReservationPerOrderStatus(string reservationId, uint orderNumber, OrderStatus orderStatus);
    string EventId { get; }
    string AreaId { get; }
}
