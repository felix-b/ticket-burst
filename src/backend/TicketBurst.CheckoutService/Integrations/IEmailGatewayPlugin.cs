using MimeKit;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Integrations;

public interface IEmailGatewayPlugin
{
    Task SendEmailMessage(MimeMessage message);
}
