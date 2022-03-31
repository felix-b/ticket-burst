using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Integrations;

[ApiController]
[Route("payment-mock")]
public class MockPaymentGatewayController : ControllerBase
{
    private readonly IPaymentGatewayPlugin _paymentPlugin;

    public MockPaymentGatewayController(IPaymentGatewayPlugin paymentPlugin)
    {
        _paymentPlugin = paymentPlugin;
    }

    [HttpPost("confirm-payment")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<string>>> ConfirmPayment([FromBody] string paymentToken)
    {
        var mockPaymentPlugin = (MockPaymentGatewayPlugin)_paymentPlugin;
        await Task.Delay(mockPaymentPlugin.GetRandomResponseTime());
        
        try
        {
            Console.WriteLine($"MockPaymentGatewayController> ConfirmPayment> PAYMENT TOKEN [{paymentToken}]");
            
            var data = mockPaymentPlugin.DecryptPaymentToken(paymentToken);
            var reply = data.NotificationStatus == "OK"
                ? "SUCCESS"
                : "FAILURE";

            Console.WriteLine($"MockPaymentGatewayController> ConfirmPayment> DECRYPTED OK reply=[{reply}]");
            
            mockPaymentPlugin.NotifyPaymentMethodReceived(paymentToken);
            return ApiResult.Success(200, reply);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e);
            return ApiResult.Error(400, "BadPaymentToken");
        }
    }
    
    [HttpGet("get-session")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<MockPaymentGatewayPlugin.PaymentData>>> GetCustomerSession([FromQuery] string sessionId)
    {
        var mockPaymentPlugin = (MockPaymentGatewayPlugin)_paymentPlugin;
        await Task.Delay(mockPaymentPlugin.GetRandomResponseTime());
        
        try
        {
            Console.WriteLine($"MockPaymentGatewayController> GetCustomerSession> PAYMENT TOKEN [{sessionId}]");
            
            var data = mockPaymentPlugin.DecryptPaymentToken(sessionId);

            Console.WriteLine($"MockPaymentGatewayController> ConfirmPayment> DECRYPTED OK");
            
            return ApiResult.Success(200, data);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e);
            return ApiResult.Error(404, "SessionNotFound");
        }
    }
}
