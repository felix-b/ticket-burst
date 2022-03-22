using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Controllers;

[ApiController]
[Route("reservation")]
public class ReservationController : ControllerBase
{
    public ReservationController(ILogger<ReservationController> logger)
    {
    }

    [HttpPost("reserve-seats")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ReplyContract<SeatReservationReplyContract>>> ReserveSeats(
        [FromBody] SeatReservationRequestContract request)
    {
        var actor = await EventAreaManagerCache.SingletonInstance.GetActor(request.EventId, request.HallAreaId);
        var response = actor?.TryReserveSeats(request);
        
        var reply = new ReplyContract<SeatReservationReplyContract>(
            response, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        var statusCode = (
            response == null 
                ? 404
                : response.Success ? 200 : 409);
        
        return new JsonResult(reply) {
            StatusCode = statusCode 
        };
    }
}
