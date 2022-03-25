using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Controllers;

[ApiController]
[Route("reservation")]
public class ReservationController : ControllerBase
{
    private readonly IDataProtector _checkoutTokenProtector;
    
    public ReservationController(
        IDataProtectionProvider protectionProvider,
        ILogger<ReservationController> logger)
    {
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
            ? await EventAreaManagerCache.SingletonInstance.GetActor(request.EventId, request.HallAreaId)
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
