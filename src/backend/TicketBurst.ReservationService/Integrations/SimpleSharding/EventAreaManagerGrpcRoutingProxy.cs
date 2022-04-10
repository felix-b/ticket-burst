using TicketBurst.Contracts;
using TicketBurst.Contracts.Proto;
using TicketBurst.Reservation.Integrations.SimpleSharding;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class EventAreaManagerGrpcRoutingProxy : IEventAreaManager
{
    public static readonly TimeSpan SteadyAwaitTimeout = TimeSpan.FromSeconds(30);
    
    private readonly SimpleStatefulClusterMember _cluster;
    private readonly SimpleShardMailbox _inprocMailbox;
    private readonly CancellationToken _cancellation;
    private readonly EventAreaManagerShardClientPool _shardClientPool;

    public EventAreaManagerGrpcRoutingProxy(
        string eventId, 
        string areaId, 
        SimpleStatefulClusterMember cluster, 
        SimpleShardMailbox inprocMailbox,
        EventAreaManagerShardClientPool shardClientPool,
        CancellationToken cancellation)
    {
        EventId = eventId;
        AreaId = areaId;
        _cluster = cluster;
        _inprocMailbox = inprocMailbox;
        _shardClientPool = shardClientPool;
        _cancellation = cancellation;
    }

    public async Task Ping()
    {
        var (action, memberIndex) = await _cluster.TriageMessageRoutingAsync(AreaId, SteadyAwaitTimeout, _cancellation);

        switch (action)
        {
            case MessageRoutingAction.HandleInProc:
                await _inprocMailbox.DispatchActionAsync<object?>(EventId, AreaId, async eam => {
                    await eam.Ping();
                    return null;
                });
                break;
            case MessageRoutingAction.RelayToMember:
                var client = GetClient(memberIndex); 
                await client.PingAsync(new PingRequest()).ResponseAsync;
                break;
            default:
                throw new Exception("Bad MessageRoutingAction");
        }
    }

    public async Task<SeatReservationReplyContract> TryReserveSeats(SeatReservationRequestContract request)
    {
        var (action, memberIndex) = await _cluster.TriageMessageRoutingAsync(AreaId, SteadyAwaitTimeout, _cancellation);

        switch (action)
        {
            case MessageRoutingAction.HandleInProc:
                return await _inprocMailbox.DispatchActionAsync(EventId, AreaId, eam => {
                    return eam.TryReserveSeats(request);
                });
            case MessageRoutingAction.RelayToMember:
                var client = GetClient(memberIndex); 
                var response = await client.TryReserveSeatsAsync(request.ToProto()).ResponseAsync;
                return response.FromProto();
            default:
                throw new Exception("Bad MessageRoutingAction");
        }
    }

    public async Task<ReservationJournalRecord?> FindEffectiveJournalRecordById(string reservationId)
    {
        var (action, memberIndex) = await _cluster.TriageMessageRoutingAsync(AreaId, SteadyAwaitTimeout, _cancellation);

        switch (action)
        {
            case MessageRoutingAction.HandleInProc:
                return await _inprocMailbox.DispatchActionAsync(EventId, AreaId, eam => {
                    return eam.FindEffectiveJournalRecordById(reservationId);
                });
            case MessageRoutingAction.RelayToMember:
                var request = new FindEffectiveJournalRecordByIdRequest {
                    Key = new ActorKey {
                        EventId = EventId,
                        AreaId = AreaId
                    },
                    ReservationId = reservationId
                };
                var client = GetClient(memberIndex); 
                var response = await client.FindEffectiveJournalRecordByIdAsync(request).ResponseAsync;
                return response.FromProto();
            default:
                throw new Exception("Bad MessageRoutingAction");
        }
    }

    public async Task<bool> UpdateReservationPerOrderStatus(string reservationId, uint orderNumber, OrderStatus orderStatus)
    {
        var (action, memberIndex) = await _cluster.TriageMessageRoutingAsync(AreaId, SteadyAwaitTimeout, _cancellation);

        switch (action)
        {
            case MessageRoutingAction.HandleInProc:
                return await _inprocMailbox.DispatchActionAsync(EventId, AreaId, eam => {
                    return eam.UpdateReservationPerOrderStatus(reservationId, orderNumber, orderStatus);
                });
            case MessageRoutingAction.RelayToMember:
                var request = new UpdateReservationPerOrderStatusRequest() {
                    Key = new ActorKey {
                        EventId = EventId,
                        AreaId = AreaId
                    },
                    ReservationId = reservationId,
                    OrderNumber = orderNumber,
                    OrderStatus = (int)orderStatus
                };
                var client = GetClient(memberIndex); 
                var response = await client.UpdateReservationPerOrderStatusAsync(request).ResponseAsync;
                return response.Updated;
            default:
                throw new Exception("Bad MessageRoutingAction");
        }
    }

    public async Task<EventAreaUpdateNotificationContract> GetUpdateNotification()
    {
        var (action, memberIndex) = await _cluster.TriageMessageRoutingAsync(AreaId, SteadyAwaitTimeout, _cancellation);

        switch (action)
        {
            case MessageRoutingAction.HandleInProc:
                return await _inprocMailbox.DispatchActionAsync(EventId, AreaId, eam => {
                    return eam.GetUpdateNotification();
                });
            default:
                throw new Exception($"EAM[{EventId}/{AreaId}]: operation '{nameof(GetUpdateNotification)}' must be invoked in-proc only");
        }
    }

    public async Task ReleaseExpiredReservations()
    {
        var (action, memberIndex) = await _cluster.TriageMessageRoutingAsync(AreaId, SteadyAwaitTimeout, _cancellation);

        switch (action)
        {
            case MessageRoutingAction.HandleInProc:
                await _inprocMailbox.DispatchActionAsync<bool>(EventId, AreaId, async eam => {
                    await eam.ReleaseExpiredReservations();
                    return true;
                });
                break;
            default:
                throw new Exception($"EAM[{EventId}/{AreaId}]: operation '{nameof(ReleaseExpiredReservations)}' must be invoked in-proc only");
        }
    }

    public string EventId { get; }
    public string AreaId { get; }

    private EventAreaManagerShard.EventAreaManagerShardClient GetClient(uint? memberIndex)
    {
        return _shardClientPool.GetClient((int)memberIndex!.Value);
    }
}
