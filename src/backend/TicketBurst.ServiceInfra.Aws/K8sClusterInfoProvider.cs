using System.Collections.Immutable;
using k8s;
using k8s.Models;

namespace TicketBurst.ServiceInfra.Aws;

public class K8sClusterInfoProvider : IClusterInfoProvider
{
    private readonly string _deploymentName;
    private readonly string _namespaceName;
    private readonly IKubernetes _client;

    public K8sClusterInfoProvider(string deploymentName, string namespaceName)
    {
        _deploymentName = deploymentName;
        _namespaceName = namespaceName;

        try
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            _client = new Kubernetes(config);
            
            Console.WriteLine("K8sClusterInfoProvider: initializing 1");
            Console.WriteLine($"K8sClusterInfoProvider: this pod name [{Environment.MachineName}]");

            var list = _client.ListNamespacedPod("default");
            if (list == null)
            {
                throw new Exception("ListNamespacedPod returned null!");
            }

            Console.WriteLine("K8sClusterInfoProvider: initializing 2");

            Current = CreateInfo(list);
            
            foreach (var item in list.Items)
            {
                Console.WriteLine(item.Metadata.Name);
            }

            Console.WriteLine("K8sClusterInfoProvider: initializing 3");
        }
        catch (Exception e)
        {
            Console.WriteLine($"K8sClusterInfoProvider FAILURE! {e.ToString()}");
        }
    }

    public string GetMemberUrl(int memberIndex)
    {
        throw new NotImplementedException();
    }

    public ClusterInfo Current { get; }
    
    public event Action? Changed;

    private ClusterInfo CreateInfo(V1PodList pods)
    {
        var thisPodName = Environment.MachineName;
        var deploymentPods = pods.Items
            .Where(p => p.Status.Phase == "Running" && p.Metadata.Name.StartsWith(_deploymentName))
            .ToArray();

        var thisPod = deploymentPods.FirstOrDefault(p => p.Metadata.Name == thisPodName);
        if (thisPod == null)
        {
            Console.WriteLine("Could not find my pod!!");
            return ClusterInfo.Empty;
        }

        return new ClusterInfo(
            MemberCount: deploymentPods.Length,
            ThisMemberIndex: Array.IndexOf(deploymentPods, thisPod),
            Generation: thisPod.Metadata.Generation.GetValueOrDefault(0),
            MemberHostNames: deploymentPods.Select(p => p.Metadata.Name).ToImmutableList()
        );
    }

    public static bool IsK8sEnvironment() => KubernetesClientConfiguration.IsInCluster();
}
