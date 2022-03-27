using System.Collections.Immutable;
using System.Net.Mail;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Contracts;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Controllers;

[ApiController]
[Route("order")]
public class OrderController : ControllerBase
{
    private readonly IMessagePublisher<OrderStatusUpdateNotificationContract> _statusUpdatePublisher;

    public OrderController(
        IMessagePublisher<OrderStatusUpdateNotificationContract> statusUpdatePublisher)
    {
        _statusUpdatePublisher = statusUpdatePublisher;
    }
    
    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<OrderContract>>> Get()
    {
        var data = MockDatabase.Orders.GetTop(20);
        return ApiResult.Success(200, data);
    }

    [HttpPost("update-status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ReplyContract<string>>> UpdateOrderStatus(
        [FromBody] UpdateOrderStatusRequest request)
    {
        if (request.OrderStatus != OrderStatus.Completed && request.OrderStatus != OrderStatus.FailedToComplete)
        {
            return ApiResult.Error(400, "BadOrderStatus");
        }

        if (string.IsNullOrWhiteSpace(request.PaymentToken))
        {
            return ApiResult.Error(400, "BadPaymentToken");
        }

        var order = await MockDatabase.Orders.TryGetByNumber(request.OrderNumber);
        if (order == null)
        {
            return ApiResult.Error(400, "OrderNotFound");
        }

        var updatedOrder = order;
        try
        {
            await MockDatabase.Orders.Update(request.OrderNumber!, oldOrder => {
                if (oldOrder.Status != OrderStatus.CompletionInProgress)
                {
                    throw new InvalidOrderStatusException();
                }
                updatedOrder = oldOrder with {
                    Status = request.OrderStatus,
                    PaymentToken = request.PaymentToken
                };
                return updatedOrder;
            });
        }
        catch (InvalidOrderStatusException)
        {
            return ApiResult.Error(409, "InvalidOrderState");
        }

        _statusUpdatePublisher.Publish(new OrderStatusUpdateNotificationContract(
            Id: MockDatabase.MakeNewId(),
            CreatedAtUtc: DateTime.UtcNow,
            UpdatedOrder: updatedOrder
        ));
        
        return ApiResult.Success(200, "OK");
    }
}
