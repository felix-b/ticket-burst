#pragma warning disable CS1998
// ReSharper disable InconsistentlySynchronizedField

using System.Collections.Immutable;
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

        public static async Task Insert(OrderContract order)
        {
            lock (__syncRoot)
            {
                __orderByNumber = __orderByNumber.Add(order.OrderNumber, order);
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
}
