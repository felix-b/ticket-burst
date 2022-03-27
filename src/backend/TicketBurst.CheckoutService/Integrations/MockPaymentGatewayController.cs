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
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        var mockPaymentPlugin = (MockPaymentGatewayPlugin)_paymentPlugin;
        
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
}
