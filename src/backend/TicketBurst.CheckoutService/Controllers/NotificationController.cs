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
    private readonly ICheckoutEntityRepository _entityRepo;

    public NotificationController(IPaymentGatewayPlugin paymentPlugin, ISagaEnginePlugin sagaEngine, ICheckoutEntityRepository entityRepo)
    {
        _paymentPlugin = paymentPlugin;
        _sagaEngine = sagaEngine;
        _entityRepo = entityRepo;
    }

    [HttpPost("payment")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> HandlePaymentNotification()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var bodyAsString = await reader.ReadToEndAsync();
        
        Console.WriteLine($"NotificationController> HandlePaymentNotification> bodyAsString [{bodyAsString}]");
        
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

        var workflowState = await _entityRepo.TryGetWorkflowStateRecord(orderNumber);
        if (workflowState == null)
        {
            return ApiResult.Error(400, "WorkflowNotFound");
        }
            
        await _sagaEngine.DispatchPaymentCompletionEvent(workflowState.AwaitStateToken, paymentToken, orderNumber, orderStatus);
        await _entityRepo.DeleteWorkflowStateRecord(orderNumber);

        return ApiResult.Success(200, "OK");
    }
}