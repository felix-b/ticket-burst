#pragma warning disable CS1998

using TicketBurst.CheckoutService.Contracts;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Integrations;

public class InMemoryCheckoutEntityRepository : ICheckoutEntityRepository
{
    public string MakeNewId()
    {
        return MockDatabase.MakeNewId();
    }

    public uint TakeNextOrderNumber()
    {
        return MockDatabase.OrderNumberCounter.TakeNextOrderNumber();
    }

    public async Task<OrderContract> InsertOrder(OrderContract order)
    {
         await MockDatabase.Orders.Insert(order);
         return order;
    }

    public async Task<IEnumerable<OrderContract>> GetMostRecentOrders(int count)
    {
        return MockDatabase.Orders.GetTop(count);
    }

    public Task<OrderContract?> TryGetOrderByNumber(uint orderNumber)
    {
        return MockDatabase.Orders.TryGetByNumber(orderNumber);
    }

    public async Task UpdateOrderPaymentStatus(uint orderNumber, OrderStatus newStatus, string newPaymentToken)
    {
        await MockDatabase.Orders.Update(orderNumber!, oldOrder => {
            if (oldOrder.Status != OrderStatus.CompletionInProgress)
            {
                throw new InvalidOrderStatusException();
            }
            var updatedOrder = oldOrder with {
                Status = newStatus,
                PaymentToken = newPaymentToken
            };
            return updatedOrder;
        });
    }
}