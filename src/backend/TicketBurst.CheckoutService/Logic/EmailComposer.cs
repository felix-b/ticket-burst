using MimeKit;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Logic;

public class EmailComposer
{
    public MimeMessage ComposeOrderCompletedEmail(OrderContract order, byte[] ticketsPdf)
    {
        return new MimeMessage();
    }

    public MimeMessage ComposeOrderFailureEmail(OrderContract order)
    {
        return new MimeMessage();
    }
}
