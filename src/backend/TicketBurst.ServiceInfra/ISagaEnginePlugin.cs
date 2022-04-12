using TicketBurst.Contracts;

namespace TicketBurst.ServiceInfra;

public interface ISagaEnginePlugin
{
    Task CreateOrderCompletionWorkflow(OrderContract order);
    Task DispatchPaymentCompletionEvent(string awaitStateToken, string paymentToken, uint orderNumber, OrderStatus orderStatus);
}
