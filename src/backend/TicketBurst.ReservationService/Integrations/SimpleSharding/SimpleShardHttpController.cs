#pragma warning disable CS8618

using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.Reservation.Integrations.SimpleSharding;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

[ApiController]
[Route("actor-shard")]
public class SimpleShardHttpController : ControllerBase
{
    private readonly IActorEngine _actorEngine;

    public SimpleShardHttpController(IActorEngine actorEngine)
    {
        _actorEngine = actorEngine;
    }

    [HttpPost("ping")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<string>>> Ping([FromBody] PingRequest request)
    {
        var actor = await GetActorOrThrow(request.EventId, request.AreaId);
        await actor.Ping();
        return ApiResult.Success(200, "OK");
    }
    
    [HttpPost("tryReserveSeats")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<SeatReservationReplyContract>>> TryReserveSeats([FromBody] SeatReservationRequestContract request)
    {
        var actor = await GetActorOrThrow(request.EventId, request.HallAreaId);
        var reply = await actor.TryReserveSeats(request);
        return ApiResult.Success(200, reply);
    }
    
    [HttpPost("findEffectiveJournalRecordById")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<ReservationJournalRecord?>>> FindEffectiveJournalRecordById([FromBody] FindEffectiveJournalRecordByIdRequest request)
    {
        var actor = await GetActorOrThrow(request.EventId, request.AreaId);
        var result = await actor.FindEffectiveJournalRecordById(request.ReservationId);
        return ApiResult.Success(200, result);
    }
    
    [HttpPost("updateReservationPerOrderStatus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<string>>> UpdateReservationPerOrderStatus([FromBody] UpdateReservationPerOrderStatusRequest request)
    {
        var actor = await GetActorOrThrow(request.EventId, request.AreaId);
        var result = await actor.UpdateReservationPerOrderStatus(request.ReservationId, request.OrderNumber, (OrderStatus)request.OrderStatus);
        return ApiResult.Success(200, result.ToString());
    }
    
    [HttpPost("getUpdateNotification")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<EventAreaUpdateNotificationContract>>> GetUpdateNotification([FromBody] GetUpdateNotificationRequest request)
    {
        var actor = await GetActorOrThrow(request.EventId, request.AreaId);
        var notification = await actor.GetUpdateNotification();
        return ApiResult.Success(200, notification);
    }

    private async Task<IEventAreaManager> GetActorOrThrow(string eventId, string areaId)
    {
        var actor = await _actorEngine.GetActor(eventId, areaId);
        return actor ?? throw new Exception($"Actor not found [{eventId}/{areaId}]");
    }

    public class PingRequest
    {
        public string EventId { get; set; }
        public string AreaId { get; set; }
    }
    
    public class FindEffectiveJournalRecordByIdRequest
    {
        public string EventId { get; set; }
        public string AreaId { get; set; }
        public string ReservationId { get; set; }
    }

    public class UpdateReservationPerOrderStatusRequest
    {
        public string EventId { get; set; }
        public string AreaId { get; set; }
        public string ReservationId { get; set; }
        public uint OrderNumber { get; set; }
        public OrderStatus OrderStatus  { get; set; }
    }

    public class GetUpdateNotificationRequest
    {
        public string EventId { get; set; }
        public string AreaId { get; set; }
    }
}
