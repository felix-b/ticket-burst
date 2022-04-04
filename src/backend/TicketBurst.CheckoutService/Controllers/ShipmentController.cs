using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Contracts;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.CheckoutService.Logic;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Controllers;

[ApiController]
[Route("shipment")]
public class ShipmentController : ControllerBase
{
    private readonly ICheckoutEntityRepository _entityRepo;
    private readonly IEmailGatewayPlugin _emailGateway;
    private readonly IStorageGatewayPlugin _storageGateway;

    public ShipmentController(
        ICheckoutEntityRepository entityRepo,
        IEmailGatewayPlugin emailGateway, 
        IStorageGatewayPlugin storageGateway)
    {
        _entityRepo = entityRepo;
        _emailGateway = emailGateway;
        _storageGateway = storageGateway;
    }

    [HttpPost("ship-tickets")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ReplyContract<string>>> ShipTickets(
        [FromBody] ShipTicketsRequest request)
    {
        var order = await _entityRepo.TryGetOrderByNumber(request.OrderNumber);
        if (order == null)
        {
            return ApiResult.Error(400, "OrderNotFound");
        }

        var renderer = new TicketRenderer();
        var ticketsPdf = renderer.RenderTicketsToPdf(order.Tickets);

        var composer = new EmailComposer();
        using var emailMessage = composer.ComposeOrderCompletedEmail(order, ticketsPdf);

        await _storageGateway.UploadObject($"orders/{order.ReservationId}/tickets.pdf", ticketsPdf);
        await _emailGateway.SendEmailMessage(emailMessage);

        return ApiResult.Success(200, "OK");
    }
}
