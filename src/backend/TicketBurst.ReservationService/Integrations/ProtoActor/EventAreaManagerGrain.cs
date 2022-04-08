using Proto;
using TicketBurst.Contracts;
using TicketBurst.Contracts.Proto;
using TicketBurst.Reservation.ProtoActor;
using TicketBurst.ReservationService.Actors;

namespace TicketBurst.ReservationService.Integrations.ProtoActor;

public class EventAreaManagerGrain : EventAreaManagerGrainBase, IDisposable
{
    private readonly ProtoActorEngine _actorEngine;
    private readonly Task _managerRecovery;

    public EventAreaManagerGrain(IContext ctx, string identity, ProtoActorEngine actorEngine, IServiceProvider services)
        : base(ctx)
    {
        Console.WriteLine($"EventAreaManagerGrain[{identity}] CTOR");

        _actorEngine = actorEngine;
        Identity = identity;
        
        EventAreaManagerGrainIdentity.ParseOrThrow(identity, out var eventId, out var areaId);
        Manager = new EventAreaManager(
            eventId,
            areaId,
            services.GetRequiredService<IReservationEntityRepository>());

        _managerRecovery = RecoverManagerState();
        _actorEngine.RegisterLocalGrain(this);
    }

    public void Dispose()
    {
        Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] DISPOSING");
        _actorEngine.UnRegisterLocalGrain(this);
    }

    public override async Task<PingResponse> Ping(PingRequest request)
    {
        await _managerRecovery;
        await Manager.Ping();
        return new PingResponse();
    }

    public override async Task<TryReserveSeatsResponse> TryReserveSeats(TryReserveSeatsRequest request)
    {
        await _managerRecovery;
        var reply = await Manager.TryReserveSeats(request.FromProto());
        return reply.ToProto();
    }

    public override async Task<FindEffectiveJournalRecordByIdResponse> FindEffectiveJournalRecordById(FindEffectiveJournalRecordByIdRequest request)
    {
        await _managerRecovery;
        var notification = await Manager.FindEffectiveJournalRecordById(request.ReservationId);
        return notification.ToProto();
    }

    public override async Task<UpdateReservationPerOrderStatusResponse> UpdateReservationPerOrderStatus(UpdateReservationPerOrderStatusRequest request)
    {
        await _managerRecovery;
        var updated = await Manager.UpdateReservationPerOrderStatus(
            request.ReservationId,
            request.OrderNumber,
            (OrderStatus)request.OrderStatus);

        return new UpdateReservationPerOrderStatusResponse() {
            Updated = updated
        };
    }

    public override async Task<GetUpdateNotificationResponse> GetUpdateNotification(GetUpdateNotificationRequest request)
    {
        await _managerRecovery;
        var notification = await Manager.GetUpdateNotification();
        return notification.ToProto();
    }

    public override async Task<ReleaseExpiredReservationsResponse> ReleaseExpiredReservations(ReleaseExpiredReservationsRequest request)
    {
        await _managerRecovery;
        await Manager.ReleaseExpiredReservations();
        return new ReleaseExpiredReservationsResponse();
    }

    public override Task OnReceive()
    {
        Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] ON-RECEIVE");
        return base.OnReceive();
    }

    public override Task OnStarted()
    {
        Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] ON-STARTED");
        return base.OnStarted();
    }

    public override Task OnStopping()
    {
        Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] ON-STOPPING");
        return base.OnStopping();
    }

    public override Task OnStopped()
    {
        Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] ON-STOPPED");
        return base.OnStopped();
    }

    public EventAreaManagerProxy GetProxyOfSelf()
    {
        var client = new EventAreaManagerGrainClient(_actorEngine.Cluster, Identity);
        var proxy = new EventAreaManagerProxy(Manager.EventId, Manager.AreaId, client);
        return proxy;
    }
    
    public EventAreaManager Manager { get; }
    public string Identity { get; }

    private async Task RecoverManagerState()
    {
        await Task.Yield();
        
        Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] STARTING RECOVERY");

        try
        {
            await Manager.RecoverState();
            Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] RECOVERY COMPLETED");
        }
        catch (Exception e)
        {
            Console.WriteLine($"EventAreaManagerGrain[{Manager.EventId}/{Manager.AreaId}] RECOVERY FAILED! {e.ToString()}");
        }
    }
}
