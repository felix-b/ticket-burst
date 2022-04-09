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

            var statefulSetList = _client.ListNamespacedStatefulSet(
                namespaceParameter: _namespaceName,
                labelSelector: $"appsvc={_serviceName}");
            
            Console.WriteLine($"K8sClusterInfoProvider: ListNamespacedStatefulSet returned {statefulSetList.Items.Count} items");

            foreach (var statefulSet in statefulSetList.Items)
            {
                Console.WriteLine("---item---");
                Console.WriteLine($"statefulSet.Spec.Replicas={statefulSet.Spec.Replicas}");
                Console.WriteLine($"statefulSet.Metadata.ClusterName={statefulSet.Metadata.ClusterName}");
                Console.WriteLine($"statefulSet.Metadata.Name={statefulSet.Metadata.Name}");
                Console.WriteLine($"statefulSet.Metadata.Generation={statefulSet.Metadata.Generation}");
                Console.WriteLine($"statefulSet.Status.ObservedGeneration={statefulSet.Status.ObservedGeneration}");
                Console.WriteLine($"statefulSet.Status.Replicas={statefulSet.Status.Replicas}");
                Console.WriteLine($"statefulSet.Status.CurrentRevision={statefulSet.Status.CurrentRevision}");
                Console.WriteLine($"statefulSet.Status.CurrentReplicas{statefulSet.Status.CurrentReplicas}");
                Console.WriteLine($"statefulSet.Status.AvailableReplicas={statefulSet.Status.AvailableReplicas}");
            }

            if (statefulSetList.Items.Count > 0)
            {
                var statefulSet = statefulSetList.Items[0];
                var memberCount = statefulSet.Spec.Replicas.GetValueOrDefault(-1); 
                Current = new ClusterInfo(
                    MemberCount: memberCount,
                    ThisMemberIndex: -1,
                    MemberHostNames: Enumerable.Range(0, memberCount)
                        .Select(i => $"{_serviceName}-{i}")
                        .ToImmutableList(),
                    Generation: statefulSet.Metadata.Generation.GetValueOrDefault(-1)
                );
            }

            Console.WriteLine("K8sClusterInfoProvider: initializing 3");
        }
        catch (Exception e)
        {
            Console.WriteLine($"K8sClusterInfoProvider FAILURE! {e.ToString()}");
        }
    }

    public ClusterInfo Current { get; } = ClusterInfo.Empty;
    
    public event Action? Changed;

    private ClusterInfo CreateInfo(V1PodList pods)
    {
        var thisPodName = Environment.MachineName;
        var podsAlive = pods.Items.Where(IsPodAlive).ToList();

        var thisPod = podsAlive.FirstOrDefault(p => p.Metadata.Name == thisPodName);
        if (thisPod == null)
        {
            Console.WriteLine("Could not find my own pod!!");
            return ClusterInfo.Empty;
        }

        return new ClusterInfo(
            MemberCount: podsAlive.Count,
            ThisMemberIndex: podsAlive.IndexOf(thisPod),
            Generation: thisPod.Metadata.Generation.GetValueOrDefault(-1),
            MemberHostNames: podsAlive.Select(p => $"{p.Metadata.Name}.{_namespaceName}").ToImmutableList()
        );
    }

    public static bool IsK8sEnvironment() => KubernetesClientConfiguration.IsInCluster();

    private static bool IsPodAlive(V1Pod pod)
    {
        return (
            pod.Status.Phase == "Running" && 
            pod.Status.PodIP is not null &&
            pod.Status.ContainerStatuses.All(s => s.Ready));
    }
}
