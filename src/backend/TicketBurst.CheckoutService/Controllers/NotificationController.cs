using System.Text;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Controllers;

[ApiController]
[Route("notify")]
public class NotificationController : ControllerBase
{
    private readonly IPaymentGatewayPlugin _paymentPlugin;
    private readonly ISagaEnginePlugin _sagaEngine;

    public NotificationController(IPaymentGatewayPlugin paymentPlugin, ISagaEnginePlugin sagaEngine)
    {
        _paymentPlugin = paymentPlugin;
        _sagaEngine = sagaEngine;
    }

    [HttpPost("payment")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> HandlePaymentNotification()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var bodyAsString = await reader.ReadToEndAsync();
        
        if (!_paymentPlugin.ParsePaymentNotification(
            bodyAsString, 
            out var paymentToken,
            out var orderNumber, 
            out var orderStatus)) 
            // out var eventId,
            // out var hallAreaId,
            // out var reservationId))
        {
            return ApiResult.Error(400);
        }

        await _sagaEngine.DispatchPaymentCompletionEvent(paymentToken, orderNumber, orderStatus);
        return ApiResult.Success(200, "OK");
    }
}

public class InvalidOrderStatusException : Exception
{
}
