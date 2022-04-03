namespace TicketBurst.ServiceInfra;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityType, string id) 
        : base($"Entity not found: {entityType}[{id}]")
    {
    }
}