using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Contracts;

public class UpdateOrderStatusRequest
{
    public UpdateOrderStatusRequest(uint orderNumber, OrderStatus orderStatus, string paymentToken)
    {
        OrderNumber = orderNumber;
        OrderStatus = orderStatus;
        PaymentToken = paymentToken;
    }

    public uint OrderNumber { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public string PaymentToken { get; set; }
}
