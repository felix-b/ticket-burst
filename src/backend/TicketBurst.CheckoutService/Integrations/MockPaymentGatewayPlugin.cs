using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Integrations;

public class MockPaymentGatewayPlugin : IPaymentGatewayPlugin
{
    private static readonly IDataProtectionProvider __protectionProvider = 
        DataProtectionProvider.Create(applicationName: typeof(MockPaymentGatewayPlugin).FullName!);

    private readonly IDataProtector _dataProtector;
    
    public MockPaymentGatewayPlugin()
    {
        _dataProtector = __protectionProvider.CreateProtector(purpose: nameof(CreatePaymentIntent));
    }

    public async Task<string> CreatePaymentIntent(OrderContract order)
    {
        var shouldFail = order.CustomerEmail.EndsWith("@failure.com");
        var data = new PaymentData(
            orderNumber: order.OrderNumber, 
            customerEmail: order.CustomerEmail, 
            customerName: order.CustomerName, 
            amount: order.PaymentTotal, 
            currency: order.PaymentCurrency,
            // eventId: order.Tickets[0].EventId,
            // hallAreaId: order.Tickets[0].HallAreaId,
            // reservationId: order.ReservationId, 
            notificationStatus: shouldFail ? "FAIL" : "OK");
        
        var json = JsonSerializer.Serialize(data);
        var paymentToken = _dataProtector.Protect(json);

        await Task.Delay(TimeSpan.FromSeconds(3));

        return paymentToken;
    }

    public bool ParsePaymentNotification(
        string body, 
        out string paymentToken,
        out uint orderNumber, 
        out OrderStatus orderStatus)
        // out string eventId,
        // out string hallAreaId, 
        // out string reservationId)
    {
        paymentToken = body;

        try
        {
            var json = _dataProtector.Unprotect(body);
            var data = JsonSerializer.Deserialize<PaymentData>(json);
            orderNumber = data!.OrderNumber;
            orderStatus = data.NotificationStatus == "OK" 
                ? OrderStatus.Completed 
                : OrderStatus.FailedToComplete;
            // eventId = data.EventId;
            // hallAreaId = data.HallAreaId;
            // reservationId = data.ReservationId;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            orderNumber = 0;
            orderStatus = OrderStatus.Preview;
            // eventId = string.Empty;
            // hallAreaId = string.Empty;
            // reservationId = string.Empty;
            return false;
        }
    }

    public void NotifyPaymentMethodReceived(string paymentToken)
    {
        Task.Factory.StartNew(async () => {
            await Task.Delay(TimeSpan.FromSeconds(20));
            await ServiceClient.HttpPostJson<string>(
                ServiceName.Checkout,
                path: new[] { "notify", "payment" },
                body: paymentToken);
        }, TaskCreationOptions.LongRunning);
    }
    //
    // public PaymentData? DecryptPaymentToken(string token)
    // {
    //     var json = _dataProtector.Unprotect(token);
    //     var data = JsonSerializer.Deserialize<PaymentData>(json);
    //     return data;
    // }

    public class PaymentData
    {
        public PaymentData(
            uint orderNumber,
            string customerEmail,
            string customerName,
            decimal amount,
            string currency,
            // string eventId,
            // string hallAreaId,
            // string reservationId,
            string notificationStatus)
        {
            OrderNumber = orderNumber;
            CustomerEmail = customerEmail;
            CustomerName = customerName;
            Amount = amount;
            Currency = currency;
            // EventId = eventId;
            // HallAreaId = hallAreaId;
            // ReservationId = reservationId;
            NotificationStatus = notificationStatus;
        }

        public uint OrderNumber { get; init; }
        public string CustomerEmail { get; init; }
        public string CustomerName { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; }
        // public string EventId { get; init; }
        // public string HallAreaId { get; init; }
        // public string ReservationId { get; init; }
        public string NotificationStatus { get; init; }
    }

    private record WorkflowEntry(
        string PaymentToken,
        TaskCompletionSource InputCompletion
    );
}
