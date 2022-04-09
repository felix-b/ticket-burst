#pragma disable CS0067

namespace TicketBurst.ServiceInfra;

public class DevboxClusterInfoProvider : IClusterInfoProvider
{
    public void Dispose()
    {
        // do nothing
    }

    public ClusterInfo Current => ClusterInfo.DevBox;
    
    public event Action? Changed;
}
