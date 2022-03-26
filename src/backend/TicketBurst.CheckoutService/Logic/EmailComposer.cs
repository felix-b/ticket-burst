using MimeKit;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Logic;

public class EmailComposer
{
    public MimeMessage ComposeOrderCompletedEmail(OrderContract order, byte[] ticketsPdf)
    {
        var message = new MimeMessage();
        message.To.Add(new MailboxAddress(name: order.CustomerName, address: order.CustomerEmail));
        message.Subject = "Congratulations! Your tickets are Ready!";
        return message;
    }

    public MimeMessage ComposeOrderFailureEmail(OrderContract order)
    {
        return new MimeMessage();
    }
}
