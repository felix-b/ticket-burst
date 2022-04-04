using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Controllers;

[ApiController]
[Route("reservation")]
public class ReservationController : ControllerBase
{
    private readonly IActorEngine _actorEngine;
    private readonly IDataProtector _checkoutTokenProtector;
    
    public ReservationController(
        IDataProtectionProvider protectionProvider,
        IActorEngine actorEngine,
        ILogger<ReservationController> logger)
    {
        _actorEngine = actorEngine;
        _checkoutTokenProtector = protectionProvider.CreateProtector(DataProtectionPurpose.CheckoutToken);
    }

    [HttpPost("grab")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ReplyContract<SeatReservationReplyContract>>> GrabSeats(
        [FromBody] SeatReservationRequestContract request)
    {
        var isValid = ValidateRequest();
        var actor = isValid
            ? await _actorEngine.GetActor(request.EventId, request.HallAreaId)
            : null;

        var response = EncryptCheckoutToken(actor?.TryReserveSeats(request));
        var reply = new ReplyContract<SeatReservationReplyContract>(
            response, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        var statusCode = (
            response == null 
                ? 400
                : response.Success ? 200 : 409);
        
        return new JsonResult(reply) {
            StatusCode = statusCode 
        };
        
        bool ValidateRequest()
        {
            return (
                !string.IsNullOrWhiteSpace(request.EventId) &&
                !string.IsNullOrWhiteSpace(request.HallAreaId) &&
                request.SeatIds != null &&
                request.SeatIds.Length > 0 &&
                !request.SeatIds.Any(string.IsNullOrWhiteSpace));
        }
    }

    [HttpGet("retrieve/{eventId}/{areaId}/{reservationId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<ReservationInfoContract>>> RetrieveReservation(
        string eventId,
        string areaId,
        string reservationId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(areaId) ||
            string.IsNullOrWhiteSpace(reservationId))
        {
            return ApiResult.Error(400);
        }
        
        var actor = await _actorEngine.GetActor(eventId, areaId);
        if (actor == null)
        {
            return ApiResult.Error(400);
        }

        ReservationJournalRecord? record = actor.FindEffectiveJournalRecordById(reservationId);
        if (record == null)
        {
            return ApiResult.Error(404);
        }

        var info = new ReservationInfoContract(
            Id: record.Id,
            EventId: record.EventId,
            HallAreaId: record.HallAreaId,
            SeatIds: record.SeatIds);
        return ApiResult.Success(200, info);
    }
    
    private SeatReservationReplyContract? EncryptCheckoutToken(SeatReservationReplyContract? reply)
    {
        if (reply == null)
        {
            return null;
        }

        if (reply.CheckoutToken == null)
        {
            return reply;
        }

        return reply with {
            CheckoutToken = _checkoutTokenProtector.Protect(reply.CheckoutToken)
        };
    }
}
