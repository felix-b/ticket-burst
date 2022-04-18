#pragma warning disable CS1998
// ReSharper disable InconsistentlySynchronizedField

using System.Collections.Immutable;
using TicketBurst.CheckoutService.Contracts;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService;

public static class MockDatabase
{
    public static string MakeNewId() => Guid.NewGuid().ToString("d");

    public static class OrderNumberCounter
    {
        private static uint __nextOrderNumber = 1001;
        
        public static uint TakeNextOrderNumber()
        {
            return Interlocked.Increment(ref __nextOrderNumber);
        }
    }

    public static class Orders
    {
        private static readonly object __syncRoot = new();
        private static ImmutableDictionary<uint, OrderContract> __orderByNumber = ImmutableDictionary<uint, OrderContract>.Empty;

        public static IEnumerable<OrderContract> GetTop(int count)
        {
            return __orderByNumber.Values.Take(count);
        }

        public static async Task<OrderContract?> TryGetByNumber(uint orderNumber)
        {
            if (__orderByNumber.TryGetValue(orderNumber, out var order))
            {
                return order;
            }

            return null;
        }

        public static async Task<OrderContract> Insert(OrderContract order)
        {
            lock (__syncRoot)
            {
                var orderWithNumber = order with {
                    OrderNumber = OrderNumberCounter.TakeNextOrderNumber()
                };
                
                __orderByNumber = __orderByNumber.Add(orderWithNumber.OrderNumber, orderWithNumber);
                return orderWithNumber;
            }
        }

        public static async Task<OrderContract> Update(uint orderNumber, Func<OrderContract, OrderContract> update)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            OrderContract? result = null;
            
            lock (__syncRoot)
            {
                if (__orderByNumber.TryGetValue(orderNumber, out var oldOrder))
                {
                    var newOrder = update(oldOrder!);
                    __orderByNumber = __orderByNumber.SetItem(orderNumber, newOrder);
                    result = newOrder;
                }
                else
                {
                    throw new KeyNotFoundException($"Order [{orderNumber}] not found");
                }
            }

            return result;
        }
    }

    public static class Workflows
    {
        private static readonly object __syncRoot = new();
        private static ImmutableDictionary<uint, WorkflowStateRecord> __workflowByOrderNumber = ImmutableDictionary<uint, WorkflowStateRecord>.Empty;

        public static void Insert(WorkflowStateRecord workflow)
        {
            lock (__syncRoot)
            {
                __workflowByOrderNumber = __workflowByOrderNumber.Add(workflow.OrderNumber, workflow);
            }
        }

        public static WorkflowStateRecord? TryGet(uint orderNumber)
        {
            return __workflowByOrderNumber.TryGetValue(orderNumber, out var workflow)
                ? workflow
                : null;
        }

        public static void Delete(uint orderNumber)
        {
            lock (__syncRoot)
            {
                __workflowByOrderNumber = __workflowByOrderNumber.Remove(orderNumber);
            }
        }
    }
}
