using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Integrations;

public interface ISagaEnginePlugin
{
    Task CreateOrderCompletionWorkflow(OrderContract order);
    Task DispatchPaymentCompletionEvent(string paymentToken, uint orderNumber, OrderStatus orderStatus);
}


