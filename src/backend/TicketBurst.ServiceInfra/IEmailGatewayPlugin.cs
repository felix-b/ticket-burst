using MimeKit;

namespace TicketBurst.ServiceInfra;

public interface IEmailGatewayPlugin
{
    Task SendEmailMessage(MimeMessage message);
}
