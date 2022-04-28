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
         var orderWithNumber = await MockDatabase.Orders.Insert(order);
         return orderWithNumber;
    }

    public async Task<IEnumerable<OrderContract>> GetMostRecentOrders(int count)
    {
        return MockDatabase.Orders.GetTop(count);
    }

    public Task<IEnumerable<AggregatedSalesRecord>> GetRecentAggregatedSales(int count)
    {
        throw new NotImplementedException();
    }

    public Task<OrderContract?> TryGetOrderByNumber(uint orderNumber)
    {
        return MockDatabase.Orders.TryGetByNumber(orderNumber);
    }

    public async Task<OrderContract> UpdateOrderPaymentStatus(uint orderNumber, OrderStatus newStatus, string newPaymentToken)
    {
        var result = await MockDatabase.Orders.Update(orderNumber!, oldOrder => {
            if (oldOrder.Status != OrderStatus.CompletionInProgress)
            {
                throw new InvalidOrderStatusException(
                    $"Expected order [{orderNumber}] in status [{OrderStatus.CompletionInProgress}]");
            }
            var updatedOrder = oldOrder with {
                Status = newStatus,
                PaymentToken = newPaymentToken,
                PaymentReceivedUtc = DateTime.UtcNow
            };
            return updatedOrder;
        });

        return result;
    }

    public async Task<OrderContract> UpdateOrderShippedStatus(uint orderNumber, DateTime shippedAtUtc)
    {
        var result = await MockDatabase.Orders.Update(orderNumber!, oldOrder => {
            if (oldOrder.Status != OrderStatus.Completed)
            {
                throw new InvalidOrderStatusException(
                    $"Expected order [{orderNumber}] in status [{OrderStatus.Completed}]");
            }
            var updatedOrder = oldOrder with {
                TicketsShippedUtc = shippedAtUtc
            };
            return updatedOrder;
        });

        return result;
    }

    public Task InsertWorkflowStateRecord(WorkflowStateRecord state)
    {
        throw new NotImplementedException();
    }

    public Task<WorkflowStateRecord?> TryGetWorkflowStateRecord(uint orderNumber)
    {
        throw new NotImplementedException();
    }

    public Task DeleteWorkflowStateRecord(uint orderNumber)
    {
        throw new NotImplementedException();
    }

    public Task UpsertAggregatedSalesRecord(AggregatedSalesRecord record)
    {
        throw new NotImplementedException();
    }
}
