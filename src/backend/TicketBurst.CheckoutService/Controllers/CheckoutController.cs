using System.Collections.Immutable;
using System.Net.Mail;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Controllers;

[ApiController]
[Route("checkout")]
public class CheckoutController : ControllerBase
{
    private static readonly string __reasonRequiredFieldsMissing = "RequiredFieldsMissing";
    private static readonly string __reasonBadCheckoutToken = "BadCheckoutToken";
    private static readonly string __reasonBadEmailAddress = "BadEmailAddress";
    
    private readonly IDataProtector _checkoutTokenProtector;
    private readonly IPaymentGatewayPlugin _paymentPlugin;
    private readonly ISagaEnginePlugin _sagaEngine;
    private readonly IMessagePublisher<OrderStatusUpdateNotificationContract> _statusUpdatePublisher;

    public CheckoutController(
        IDataProtectionProvider protectionProvider, 
        IPaymentGatewayPlugin paymentPlugin,
        ISagaEnginePlugin sagaEngine,
        IMessagePublisher<OrderStatusUpdateNotificationContract> statusUpdatePublisher)
    {
        _paymentPlugin = paymentPlugin;
        _sagaEngine = sagaEngine;
        _statusUpdatePublisher = statusUpdatePublisher;
        _checkoutTokenProtector = protectionProvider.CreateProtector(DataProtectionPurpose.CheckoutToken);
    }

    [HttpPost("begin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<OrderContract>>> BeginCheckout(
        [FromBody] BeginCheckoutRequest request)
    {
        var isPreviewMode = request.Preview == true;
        if (!ValidateRequest(
            out var validEventId, 
            out var validAreaId, 
            out var validReservationId, 
            out var validationReason))
        {
            return ApiResult.Error(400, validationReason);
        }

        var reservationInfo = await TryRetrieveReservation(); 
        if (reservationInfo == null)
        {
            return ApiResult.Error(400, "ReservationNotFound");
        }

        var tickets = await TryRetrieveTickets();
        if (tickets == null || tickets.Count == 0)
        {
            return ApiResult.Error(400, "TicketsNotAvailable");
        }

        var order = await CreateOrder();
        if (!isPreviewMode)
        {
            await MockDatabase.Orders.Insert(order);
            await _sagaEngine.CreateOrderCompletionWorkflow(order);
        }
        
        return ApiResult.Success(200, order with {
            ReservationId = string.Empty
        });
        
        async Task<ReservationInfoContract?> TryRetrieveReservation()
        {
            return await ServiceClient.HttpGetJson<ReservationInfoContract>(
                ServiceName.Reservation,
                path: new[] { "reservation", "retrieve", validEventId!, validAreaId!, validReservationId! });
        }
        
        async Task<ImmutableList<TicketContract>?> TryRetrieveTickets()
        {
            return (await ServiceClient.HttpPostJson<IEnumerable<TicketContract>>(
                ServiceName.Search,
                path: new[] { "ticket", "create" },
                body: reservationInfo
            ))?.ToImmutableList();
        }
        
        bool ValidateRequest(out string eventId, out string hallAreaId, out string reservationId, out string reason)
        {
            eventId = request.EventId ?? string.Empty;
            hallAreaId = request.HallAreaId ?? string.Empty;
            reservationId = string.Empty;
            
            if (string.IsNullOrWhiteSpace(request.EventId) ||
                string.IsNullOrWhiteSpace(request.HallAreaId) ||
                string.IsNullOrWhiteSpace(request.CheckoutToken))
            {
                reason = __reasonRequiredFieldsMissing;
                return false;
            }

            if (!isPreviewMode)
            {
                if (string.IsNullOrWhiteSpace(request.CustomerName) ||
                    string.IsNullOrWhiteSpace(request.CustomerEmail))
                {
                    reason = __reasonRequiredFieldsMissing;
                    return false;
                }

                if (!IsValidEmailAddress(request.CustomerEmail))
                {
                    reason = __reasonBadEmailAddress;
                    return false;
                }
            }

            if (!DecryptReservationId(request.CheckoutToken, out reservationId))
            {
                reason = __reasonBadCheckoutToken;
                return false;
            }

            reason = string.Empty;
            return true;
        }

        bool DecryptReservationId(string checkoutToken, out string decryptedReservationId)
        {
            try
            {
                decryptedReservationId = _checkoutTokenProtector.Unprotect(checkoutToken);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                decryptedReservationId = string.Empty;
                return false;
            }
        }

        bool IsValidEmailAddress(string? email)
        {
            return (
                email != null && 
                MailAddress.TryCreate(email, out _));
        }

        async Task<OrderContract> CreateOrder()
        {
            var orderNumber = isPreviewMode 
                ? 0 
                : MockDatabase.OrderNumberCounter.TakeNextOrderNumber();
            var orderDescription =
                $"{tickets!.Count} Tickets to ${tickets[0].ShowTitle} {tickets[0].EventTitle}".Trim();

            var subtotalPrice = tickets.Sum(t => t.Price);
            var taxPrice = subtotalPrice * 0.17m;
            var totalPrice = Math.Round(subtotalPrice + taxPrice, decimals: 1, MidpointRounding.ToZero);
            var currencySymbol = "USD";

            var orderWithoutPaymentToken = new OrderContract(
                OrderNumber: orderNumber,
                Status: isPreviewMode ? OrderStatus.Preview : OrderStatus.CompletionInProgress,
                OrderDescription: orderDescription,
                CreatedAtUtc: DateTime.UtcNow,
                CustomerName: request.CustomerName!,
                CustomerEmail: request.CustomerEmail!,
                Tickets: tickets!,
                PaymentCurrency: currencySymbol,
                PaymentSubtotal: subtotalPrice,
                PaymentTax: taxPrice,
                PaymentTotal: totalPrice,
                PaymentToken: string.Empty,
                ReservationId: reservationInfo.Id);

            if (isPreviewMode)
            {
                return orderWithoutPaymentToken;
            }
            
            var paymentToken = await _paymentPlugin.CreatePaymentIntent(orderWithoutPaymentToken);
            return orderWithoutPaymentToken with {
                PaymentToken = paymentToken
            };
        }
    }

    public class BeginCheckoutRequest
    {
        public bool? Preview { get; set; }
        public string? EventId { get; set; }
        public string? HallAreaId { get; set; }
        public string? CheckoutToken { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
