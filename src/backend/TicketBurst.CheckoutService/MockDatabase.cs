#pragma warning disable CS1998

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

        public static async Task<OrderContract?> TryGetByNumber(uint orderNumber)
        {
            // ReSharper disable once InconsistentlySynchronizedField
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

        public static async Task<bool> Update(uint orderNumber, Func<OrderContract, OrderContract> update)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var result = false;
            
            lock (__syncRoot)
            {
                if (__orderByNumber.TryGetValue(orderNumber, out var oldOrder))
                {
                    var newOrder = update(oldOrder!);
                    __orderByNumber = __orderByNumber.SetItem(orderNumber, newOrder);
                    result = true;
                }
            }

            return result;
        }
    }
}
