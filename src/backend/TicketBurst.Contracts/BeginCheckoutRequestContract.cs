namespace TicketBurst.Contracts;

public class BeginCheckoutRequestContract
{
    public bool? Preview { get; set; }
    public string? EventId { get; set; }
    public string? HallAreaId { get; set; }
    public string? CheckoutToken { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
}
