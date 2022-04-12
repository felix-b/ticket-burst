namespace TicketBurst.CheckoutService.Contracts;

public class WorkflowStateRecord
{
    public WorkflowStateRecord(uint orderNumber, string awaitStateName, string awaitStateToken)
    {
        OrderNumber = orderNumber;
        AwaitStateName = awaitStateName;
        AwaitStateToken = awaitStateToken;
    }

    public virtual uint OrderNumber { get; set; }
    public virtual string AwaitStateName { get; set; }
    public virtual string AwaitStateToken { get; set; }
}
