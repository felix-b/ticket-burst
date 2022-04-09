using System.Collections.Immutable;
using k8s;
using k8s.Models;
using Microsoft.Rest;

namespace TicketBurst.ServiceInfra.Aws;

public class K8sClusterInfoProvider : IClusterInfoProvider
{
    private readonly string _serviceName;
    private readonly string _namespaceName;
    private readonly IKubernetes _client;

    public K8sClusterInfoProvider(string serviceName, string namespaceName)
    {
        _serviceName = serviceName;
        _namespaceName = namespaceName;

        try
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            _client = new Kubernetes(config);
            
            Console.WriteLine("K8sClusterInfoProvider: initializing 1");
            Console.WriteLine($"K8sClusterInfoProvider: this pod name [{Environment.MachineName}]");

            var list = _client.ListNamespacedPod(namespaceParameter: _namespaceName, labelSelector: $"appsvc={_serviceName}");
            if (list == null)
            {
                throw new Exception("ListNamespacedPod returned null!");
            }

            Current = CreateInfo(list);

            Console.WriteLine("K8sClusterInfoProvider: initializing 3");
        }
        catch (Exception e)
        {
            Console.WriteLine($"K8sClusterInfoProvider FAILURE! {e.ToString()}");
        }
    }

    public ClusterInfo Current { get; }
    
    public event Action? Changed;

    private ClusterInfo CreateInfo(V1PodList pods)
    {
        var thisPodName = Environment.MachineName;

        var thisPod = pods.Items.FirstOrDefault(p => p.Metadata.Name == thisPodName);
        if (thisPod == null)
        {
            Console.WriteLine("Could not find my own pod!!");
            return ClusterInfo.Empty;
        }

        return new ClusterInfo(
            MemberCount: pods.Items.Count,
            ThisMemberIndex: pods.Items.IndexOf(thisPod),
            Generation: thisPod.Metadata.Generation.GetValueOrDefault(-1),
            MemberHostNames: pods.Items.Select(p => $"{p.Metadata.Name}.{_namespaceName}").ToImmutableList()
        );
    }

    public static bool IsK8sEnvironment() => KubernetesClientConfiguration.IsInCluster();
}
