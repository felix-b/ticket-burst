using k8s;

namespace TicketBurst.ServiceInfra.Aws;

public class K8sClusterInfoProvider : IClusterInfoProvider
{
    private readonly IKubernetes _client;

    public K8sClusterInfoProvider()
    {
        try
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            _client = new Kubernetes(config);
            
            Console.WriteLine("K8sClusterInfoProvider: initializing 1");

            var list = _client.ListNamespacedPod("default");

            Console.WriteLine("K8sClusterInfoProvider: initializing 2");
            
            foreach (var item in list.Items)
            {
                Console.WriteLine(item.Metadata.Name);
            }

            Console.WriteLine("K8sClusterInfoProvider: initializing 3");

            Current = new ClusterInfo(0, 0, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine($"K8sClusterInfoProvider FAILURE! {e.ToString()}");
            throw;
        }
    }

    public string GetMemberUrl(int memberIndex)
    {
        throw new NotImplementedException();
    }

    public ClusterInfo Current { get; }
    
    public event Action? Changed;
}
