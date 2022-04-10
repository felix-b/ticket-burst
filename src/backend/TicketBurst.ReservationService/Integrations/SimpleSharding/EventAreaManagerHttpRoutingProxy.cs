using TicketBurst.Contracts;
using TicketBurst.Reservation.Integrations.SimpleSharding;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class EventAreaManagerHttpRoutingProxy : IEventAreaManager
{
    public static readonly TimeSpan SteadyAwaitTimeout = TimeSpan.FromSeconds(30);

    private static readonly string __actorShardRoute = "actor-shard";
    private static readonly string[] __pingPath = new[] { __actorShardRoute, "ping" };
    private static readonly string[] __tryReserveSeatsPath = new[] { __actorShardRoute, "tryReserveSeats" };
    private static readonly string[] __findEffectiveJournalRecordByIdPath = new[] { __actorShardRoute, "findEffectiveJournalRecordById" };
    private static readonly string[] __updateReservationPerOrderStatusPath = new[] { __actorShardRoute, "updateReservationPerOrderStatus" };
    private static readonly string[] __getUpdateNotificationPath = new[] { __actorShardRoute, "getUpdateNotification" };

    private readonly SimpleStatefulClusterMember _cluster;
    private readonly SimpleShardMailbox _inprocMailbox;
    private readonly CancellationToken _cancellation;

    public EventAreaManagerHttpRoutingProxy(
        string eventId, 
        string areaId, 
        SimpleStatefulClusterMember cluster, 
        SimpleShardMailbox inprocMailbox,
        CancellationToken cancellation)
    {
        EventId = eventId;
        AreaId = areaId;
        _cluster = cluster;
        _inprocMailbox = inprocMailbox;
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
                await ServiceClient.HttpPostJson<string>(
                    ServiceName.Reservation, 
                    __pingPath, 
                    body: new SimpleShardHttpController.PingRequest {
                        EventId = EventId,
                        AreaId = AreaId
                    },
                    hostOverride: GetMemberUrl(memberIndex));
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
                var reply = await ServiceClient.HttpPostJson<SeatReservationReplyContract>(
                    ServiceName.Reservation, 
                    __tryReserveSeatsPath, 
                    body: request,
                    hostOverride: GetMemberUrl(memberIndex));
                return reply!;
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
                var request = new SimpleShardHttpController.FindEffectiveJournalRecordByIdRequest {
                    EventId = EventId,
                    AreaId = AreaId,
                    ReservationId = reservationId
                };
                var reply = await ServiceClient.HttpPostJson<ReservationJournalRecord>(
                    ServiceName.Reservation, 
                    __findEffectiveJournalRecordByIdPath, 
                    body: request,
                    hostOverride: GetMemberUrl(memberIndex));
                return reply;
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
                var request = new SimpleShardHttpController.UpdateReservationPerOrderStatusRequest() {
                    EventId = EventId,
                    AreaId = AreaId,
                    ReservationId = reservationId,
                    OrderNumber = orderNumber,
                    OrderStatus = orderStatus
                };
                var reply = await ServiceClient.HttpPostJson<string>(
                    ServiceName.Reservation, 
                    __updateReservationPerOrderStatusPath, 
                    body: request,
                    hostOverride: GetMemberUrl(memberIndex));
                return bool.Parse(reply ?? "false");
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
            case MessageRoutingAction.RelayToMember:
                var request = new SimpleShardHttpController.GetUpdateNotificationRequest() {
                    EventId = EventId,
                    AreaId = AreaId,
                };
                var reply = await ServiceClient.HttpPostJson<EventAreaUpdateNotificationContract>(
                    ServiceName.Reservation, 
                    __getUpdateNotificationPath, 
                    body: request,
                    hostOverride: GetMemberUrl(memberIndex));
                return reply!;
            default:
                throw new Exception("Bad MessageRoutingAction");
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

    private string GetMemberUrl(uint? memberIndex)
    {
        var intIndex = (int)(memberIndex!.Value);
        var host = _cluster.CurrentState.MemberHostNames[intIndex];
        var url = _cluster.InfoProvider.GetEndpointUrl(host, intIndex);
        return url;
    }
}
