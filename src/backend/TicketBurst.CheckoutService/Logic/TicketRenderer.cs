using System.Text;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Logic;

public class TicketRenderer
{
    public byte[] RenderTicketsToPdf(IEnumerable<TicketContract> tickets)
    {
        return Encoding.ASCII.GetBytes("Not implemented");
    }
}
