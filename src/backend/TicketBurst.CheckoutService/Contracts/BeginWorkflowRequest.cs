namespace TicketBurst.CheckoutService.Contracts;

public class BeginWorkflowRequest
{
    public uint OrderNumber { get; set; }
    public string StateName { get; set; }
    public string TaskToken { get; set; }
}