﻿using MimeKit;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Logic;

public class EmailComposer
{
    public MimeMessage ComposeOrderCompletedEmail(OrderContract order, byte[] ticketsPdf)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(name: "TicketBurst POC", address: "info@tixlab.app"));
        message.To.Add(new MailboxAddress(name: order.CustomerName, address: order.CustomerEmail));
        message.Subject = "Summer Olympics 2024 - Order Confirmation and Tickets";

        var builder = new BodyBuilder();
        builder.TextBody = 
            $"Hello {order.CustomerName},\n\n" + 
            $"Thank you again for your purchase (order {order.OrderNumber}).\n" +
            "Please find the tickets attached.\n\n" +
            "Best regards," +
            "TicketBurst POC";

        builder.Attachments.Add("tickets.pdf", ticketsPdf);
        
        message.Body = builder.ToMessageBody();
        
        return message;
    }

    public MimeMessage ComposeOrderFailureEmail(OrderContract order)
    {
        return new MimeMessage();
    }
}
