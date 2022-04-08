using TicketBurst.Contracts;

namespace TicketBurst.ReservationService.Contracts;

public interface IEventAreaManager
{
    Task Ping();
    Task<SeatReservationReplyContract> TryReserveSeats(SeatReservationRequestContract request);
    Task<ReservationJournalRecord?> FindEffectiveJournalRecordById(string reservationId);
    Task<bool> UpdateReservationPerOrderStatus(string reservationId, uint orderNumber, OrderStatus orderStatus);
    Task<EventAreaUpdateNotificationContract> GetUpdateNotification();
    Task ReleaseExpiredReservations();
    string EventId { get; }
    string AreaId { get; }
}
