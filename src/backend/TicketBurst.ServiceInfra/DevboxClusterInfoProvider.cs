namespace TicketBurst.ServiceInfra;

public class DevboxClusterInfoProvider : IClusterInfoProvider
{
    public string GetMemberUrl(int memberIndex)
    {
        throw new NotImplementedException();
    }

    public ClusterInfo Current => throw new NotImplementedException();
    
    public event Action? Changed;
}
