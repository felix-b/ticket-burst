using TicketBurst.CheckoutService.Contracts;
using TicketBurst.CheckoutService.Controllers;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Integrations;

public class MockSagaEnginePlugin : ISagaEnginePlugin
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<uint, WorkflowEntry> _entryByOrderNumber = new();

    public async Task CreateOrderCompletionWorkflow(OrderContract order)
    {
        Console.WriteLine($"MockSagaEnginePlugin> CREATE order[{order.OrderNumber}]");
        
        var paymentCompletion = new TaskCompletionSource<OrderStatus>();
        var workflow = RunWorkflow(order, paymentCompletion.Task);
        var entry = new WorkflowEntry(
            Order: order,
            Workflow: workflow,
            PaymentCompletion: paymentCompletion);
                
        lock (_syncRoot)
        {
            _entryByOrderNumber.Add(order.OrderNumber, entry);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(100));
    }

    public Task DispatchPaymentCompletionEvent(string paymentToken, uint orderNumber, OrderStatus orderStatus)
    {
        Console.WriteLine($"MockSagaEnginePlugin> PAYMENT-EVENT order[{orderNumber}]");

        lock (_syncRoot)
        {
            if (!_entryByOrderNumber.ContainsKey(orderNumber))
            {
                Console.WriteLine($"MockSagaEnginePlugin> --> WORKFLOW NOT FOUND");
                throw new ArgumentException(
                    $"Order not found in MockSagaEnginePlugin: {orderNumber}",
                    paramName: nameof(orderNumber));
            }

            Console.WriteLine($"MockSagaEnginePlugin> --> OK");
            var entry = _entryByOrderNumber[orderNumber];
            entry.PaymentCompletion.SetResult(orderStatus);
        }

        return Task.Delay(TimeSpan.FromMilliseconds(100));
    }
    
    private async Task RunWorkflow(OrderContract order, Task<OrderStatus> paymentCompletion)
    {
        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> STARTING");

        var orderStatus = await paymentCompletion;

        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> PAYMENT FINISHED [{orderStatus}], PROCEEDING");
        
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        var updateRequest = new UpdateOrderStatusRequest(order.OrderNumber, orderStatus, order.PaymentToken);
        await ServiceClient.HttpPostJson<string>(
            ServiceName.Checkout,
            path: new[] { "order", "update-status" },
            body: updateRequest);

        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> NOTIFICATION PUBLISHED");
        
        if (orderStatus == OrderStatus.Completed)
        {
            Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> SENDING SHIPPING REQUEST");

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var shipmentRequest = new ShipTicketsRequest(order.OrderNumber);
            await ServiceClient.HttpPostJson<string>(
                ServiceName.Checkout,
                path: new[] { "shipment", "ship-tickets" },
                body: shipmentRequest);

            Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> SHIPPING REQUEST SENT");
        }

        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> FINISHED");
    }

    private record WorkflowEntry(
        OrderContract Order,
        Task Workflow,
        TaskCompletionSource<OrderStatus> PaymentCompletion)
    {
    }
}
