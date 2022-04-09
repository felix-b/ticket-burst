#pragma disable CS0067

namespace TicketBurst.ServiceInfra;

public class DevboxClusterInfoProvider : IClusterInfoProvider
{
    public ClusterInfo Current => ClusterInfo.DevBox;
    
    public event Action? Changed;
}
