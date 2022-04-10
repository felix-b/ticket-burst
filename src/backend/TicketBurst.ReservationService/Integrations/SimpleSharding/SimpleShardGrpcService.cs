using Grpc.Core;
using TicketBurst.Contracts;
using TicketBurst.Contracts.Proto;
using TicketBurst.Reservation.Integrations.SimpleSharding;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class SimpleShardGrpcService : EventAreaManagerShard.EventAreaManagerShardBase
{
    private readonly IActorEngine _actorEngine;

    public SimpleShardGrpcService(IActorEngine actorEngine)
    {
        _actorEngine = actorEngine;
    }

    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        var actor = await GetActorOrThrow(request.Key);
        await actor.Ping();
        return new PingResponse();
    }

    public override async Task<GetUpdateNotificationResponse> GetUpdateNotification(GetUpdateNotificationRequest request, ServerCallContext context)
    {
        var actor = await GetActorOrThrow(request.Key);
        var notification = await actor.GetUpdateNotification();
        return notification.ToProto();
    }

    public override async Task<ReleaseExpiredReservationsResponse> ReleaseExpiredReservations(ReleaseExpiredReservationsRequest request, ServerCallContext context)
    {
        throw new NotSupportedException($"Operation '{nameof(ReleaseExpiredReservations)}' must be invoked in-proc");
    }

    public override async Task<TryReserveSeatsResponse> TryReserveSeats(TryReserveSeatsRequest request, ServerCallContext context)
    {
        var actor = await GetActorOrThrow(request.Key);
        var reply = await actor.TryReserveSeats(request.FromProto());
        return reply.ToProto();
    }

    public override async Task<UpdateReservationPerOrderStatusResponse> UpdateReservationPerOrderStatus(UpdateReservationPerOrderStatusRequest request, ServerCallContext context)
    {
        var actor = await GetActorOrThrow(request.Key);
        var result = await actor.UpdateReservationPerOrderStatus(request.ReservationId, request.OrderNumber, (OrderStatus)request.OrderStatus);
        return new UpdateReservationPerOrderStatusResponse {
            Updated = result
        };
    }

    public override async Task<FindEffectiveJournalRecordByIdResponse> FindEffectiveJournalRecordById(FindEffectiveJournalRecordByIdRequest request, ServerCallContext context)
    {
        var actor = await GetActorOrThrow(request.Key);
        var result = await actor.FindEffectiveJournalRecordById(request.ReservationId);
        return result.ToProto();
    }

    private async Task<IEventAreaManager> GetActorOrThrow(ActorKey key)
    {
        var actor = await _actorEngine.GetActor(key.EventId, key.AreaId);
        return actor ?? throw new Exception($"Actor not found");
    }
}
