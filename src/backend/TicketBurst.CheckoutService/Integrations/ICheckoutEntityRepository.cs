using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Integrations;

public interface ICheckoutEntityRepository
{
    string MakeNewId();
    uint TakeNextOrderNumber();
    Task<OrderContract> InsertOrder(OrderContract order);
    Task<IEnumerable<OrderContract>> GetMostRecentOrders(int count);
    Task<OrderContract?> TryGetOrderByNumber(uint orderNumber);
    Task UpdateOrderPaymentStatus(uint orderNumber, OrderStatus newStatus, string newPaymentToken);
}
