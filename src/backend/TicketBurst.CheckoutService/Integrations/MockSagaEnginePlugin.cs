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
        
        var paymentCompletion = new TaskCompletionSource<PaymentCompletionEvent>();
        var workflow = RunWorkflow(order, paymentCompletion.Task);
        var entry = new WorkflowEntry(
            Order: order,
            Workflow: workflow,
            PaymentCompletion: paymentCompletion);
                
        lock (_syncRoot)
        {
            _entryByOrderNumber.Add(order.OrderNumber, entry);
        }

        var beginRequest = new BeginWorkflowRequest {
            OrderNumber = order.OrderNumber,
            StateName = "AwaitPayment",
            TaskToken = Guid.NewGuid().ToString()
        };
        await ServiceClient.HttpPostJson<string>(
            ServiceName.Checkout,
            path: new[] { "workflow", "begin" },
            body: beginRequest);

        await Task.Delay(TimeSpan.FromMilliseconds(100));
    }

    public Task DispatchPaymentCompletionEvent(string awaitStateToken, string paymentToken, uint orderNumber, OrderStatus orderStatus)
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
            var paymentEvent = new PaymentCompletionEvent(orderNumber, orderStatus, paymentToken);
            entry.PaymentCompletion.SetResult(paymentEvent);
        }

        return Task.Delay(TimeSpan.FromMilliseconds(100));
    }
    
    private async Task RunWorkflow(OrderContract order, Task<PaymentCompletionEvent> paymentCompletion)
    {
        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> STARTING");

        var paymentEvent = await paymentCompletion;

        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> PAYMENT FINISHED [{paymentEvent.OrderNumber}], PROCEEDING");
        
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        var updateRequest = new UpdateOrderStatusRequest(
            paymentEvent.OrderNumber, 
            paymentEvent.OrderStatus, 
            paymentEvent.PaymentToken);
        await ServiceClient.HttpPostJson<string>(
            ServiceName.Checkout,
            path: new[] { "order", "update-status" },
            body: updateRequest);

        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> NOTIFICATION PUBLISHED");
        
        if (paymentEvent.OrderStatus == OrderStatus.Completed)
        {
            Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> SENDING SHIPPING REQUEST");

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            var shipmentRequest = new ShipTicketsRequest(order.OrderNumber);
            await ServiceClient.HttpPostJson<string>(
                ServiceName.Checkout,
                path: new[] { "shipment", "ship-tickets" },
                body: shipmentRequest);

            var reportingRequest = new CheckoutWorkflowStateContract.OrderPart(order);
            await ServiceClient.HttpPostJson<string>(
                ServiceName.Checkout,
                path: new[] { "reporting", "add-order" },
                body: reportingRequest);
            
            Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> SHIPPING REQUEST SENT");
        }

        Console.WriteLine($"MockSagaEnginePlugin::Workflow[{order.OrderNumber}]> FINISHED");
    }

    private record WorkflowEntry(
        OrderContract Order,
        Task Workflow,
        TaskCompletionSource<PaymentCompletionEvent> PaymentCompletion)
    {
    }

    private record PaymentCompletionEvent(
        uint OrderNumber,
        OrderStatus OrderStatus,
        string PaymentToken
    );
}
