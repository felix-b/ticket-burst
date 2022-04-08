using Proto;
using TicketBurst.Contracts;
using TicketBurst.Contracts.Proto;
using TicketBurst.Reservation.ProtoActor;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations.ProtoActor;

public class EventAreaManagerProxy : IEventAreaManager
{
    private readonly EventAreaManagerGrainClient _client;

    public EventAreaManagerProxy(
        string eventId, 
        string areaId, 
        EventAreaManagerGrainClient client)
    {
        EventId = eventId;
        AreaId = areaId;
        _client = client;
    }

    public async Task Ping()
    {
        await _client.Ping(new PingRequest(), CancellationToken.None);
    }
    
    public async Task<SeatReservationReplyContract> TryReserveSeats(SeatReservationRequestContract request)
    {
        var response = await _client.TryReserveSeats(request.ToProto(), CancellationToken.None);
        return 
            response?.FromProto() 
            ?? throw new TimeoutException($"EAM[{EventId}/{AreaId}] Timed out invoking '{nameof(TryReserveSeats)}' on remote actor");
    }

    public async Task<EventAreaUpdateNotificationContract> GetUpdateNotification()
    {
        var response = await _client.GetUpdateNotification(new GetUpdateNotificationRequest(), CancellationToken.None);
        return 
            response?.FromProto() 
            ?? throw new TimeoutException($"EAM[{EventId}/{AreaId}] Timed out invoking '{nameof(GetUpdateNotification)}' on remote actor");
    }

    public async Task ReleaseExpiredReservations()
    {
        var response = await _client.ReleaseExpiredReservations(new ReleaseExpiredReservationsRequest(), CancellationToken.None);
        if (response == null)
        {
            throw new TimeoutException(
                $"EAM[{EventId}/{AreaId}] Timed out invoking '{nameof(ReleaseExpiredReservations)}' on remote actor");
        }
    }

    public async Task<ReservationJournalRecord?> FindEffectiveJournalRecordById(string reservationId)
    {
        var request = new FindEffectiveJournalRecordByIdRequest {
            ReservationId = reservationId
        };

        var response = await _client.FindEffectiveJournalRecordById(request, CancellationToken.None);
        if (response == null)
        {
            throw new TimeoutException($"EAM[{EventId}/{AreaId}] Timed out invoking '{nameof(FindEffectiveJournalRecordById)}' on remote actor");
        }

        return response.FromProto();
    }

    public async Task<bool> UpdateReservationPerOrderStatus(string reservationId, uint orderNumber, OrderStatus orderStatus)
    {
        var request = new UpdateReservationPerOrderStatusRequest {
            ReservationId = reservationId,
            OrderNumber = orderNumber,
            OrderStatus = (int)orderStatus
        };
        
        var response = await _client.UpdateReservationPerOrderStatus(request, CancellationToken.None);
        if (response == null)
        {
            throw new TimeoutException($"EAM[{EventId}/{AreaId}] Timed out invoking '{nameof(UpdateReservationPerOrderStatus)}' on remote actor");
        }

        return response.Updated;
    }

    public string EventId { get; }
    public string AreaId { get; }
}
