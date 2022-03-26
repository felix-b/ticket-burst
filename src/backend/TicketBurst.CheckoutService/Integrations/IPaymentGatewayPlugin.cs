using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Integrations;

public interface IPaymentGatewayPlugin
{
    Task<string> CreatePaymentIntent(OrderContract order);

    bool ParsePaymentNotification(
        string body, 
        out string paymentToken,
        out uint orderNumber, 
        out OrderStatus orderStatus);
}
