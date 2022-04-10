using System.Collections.Immutable;
using System.Diagnostics;
using k8s;
using k8s.Models;
using k8s.Util.Common;
using Microsoft.Rest;

namespace TicketBurst.ServiceInfra.Aws;

public class K8sClusterInfoProvider : IClusterInfoProvider
{
    private readonly string _machineName;
    private readonly int _replicaIndex;
    private readonly string _serviceName;
    private readonly string _statefulSetName;
    private readonly string _namespaceName;
    private readonly IKubernetes _client;

    public K8sClusterInfoProvider(string serviceName, string namespaceName)
    {
        _serviceName = serviceName;
        _statefulSetName = $"{_serviceName}-statefulset";
        _namespaceName = namespaceName;
        _machineName = Environment.MachineName;
        
        var config = KubernetesClientConfiguration.BuildDefaultConfig();
        _client = new Kubernetes(config);
        
        Console.WriteLine("K8sClusterInfoProvider: initializing");
        Console.WriteLine($"> this pod name [{_machineName}]");

        ParseReplicaIndexOrThrow(_machineName, out _replicaIndex);
        Console.WriteLine($"> parsed replica index [{_replicaIndex}]");
    }

    public void Dispose()
    {
    }

    public ClusterInfo? TryGetInfo()
    {
        Console.WriteLine($"K8sClusterInfoProvider> querying stateful set [{_statefulSetName}]");
                
        var statefulSet = _client.ReadNamespacedStatefulSet(_statefulSetName, _namespaceName);
        Console.WriteLine($"> {(statefulSet != null ? "success" : "FAILURE")}");

        if (statefulSet != null)
        {
            PrintStatefulSet(statefulSet);
            return CreateInfo(statefulSet);
        }
        
        return null;
    }

    public DateTime UtcNow => DateTime.UtcNow;
    
    public string GetEndpointUrl(string memberHostName, int memberIndex)
    {
        return $"http://{memberHostName}.{_namespaceName}";
    }

    private void PrintStatefulSet(V1StatefulSet statefulSet)
    {
        Console.WriteLine($"------statefulset[{statefulSet.Namespace()}/{statefulSet.Name()}]------");
        Console.WriteLine($"> statefulSet.Spec.Replicas={statefulSet.Spec.Replicas}");
        Console.WriteLine($"> statefulSet.Metadata.ClusterName={statefulSet.Metadata.ClusterName}");
        Console.WriteLine($"> statefulSet.Metadata.Name={statefulSet.Metadata.Name}");
        Console.WriteLine($"> statefulSet.Metadata.Generation={statefulSet.Metadata.Generation}");
        Console.WriteLine($"> statefulSet.Status.ObservedGeneration={statefulSet.Status.ObservedGeneration}");
        Console.WriteLine($"> statefulSet.Metadata.CreationTimestamp={statefulSet.Metadata.CreationTimestamp}");
        Console.WriteLine($"> statefulSet.Status.Replicas={statefulSet.Status.Replicas}");
        Console.WriteLine($"> statefulSet.Status.CurrentRevision={statefulSet.Status.CurrentRevision}");
        Console.WriteLine($"> statefulSet.Status.CurrentReplicas{statefulSet.Status.CurrentReplicas}");
        Console.WriteLine($"> statefulSet.Status.AvailableReplicas={statefulSet.Status.AvailableReplicas}");
    }

    private ClusterInfo CreateInfo(V1StatefulSet statefulSet)
    {
        var memberCount = statefulSet.Spec.Replicas.GetValueOrDefault(-1); 

        return new ClusterInfo(
            MemberCount: memberCount,
            ThisMemberIndex: _replicaIndex,
            MemberHostNames: Enumerable.Range(0, memberCount)
                .Select(i => $"{statefulSet.Name()}-{i}")
                .ToImmutableList(),
            Generation: statefulSet.Metadata.Generation.GetValueOrDefault(-1)
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

    private static void ParseReplicaIndexOrThrow(string statefulSetPodName, out int replicaIndex)
    {
        var lastHyphenIndex = statefulSetPodName.LastIndexOf('-');
        if (lastHyphenIndex > 0 && lastHyphenIndex < statefulSetPodName.Length - 1)
        {
            if (Int32.TryParse(statefulSetPodName.Substring(lastHyphenIndex + 1), out replicaIndex))
            {
                return;
            }
        }

        throw new InvalidDataException(
            $"Could not parse replica index from statefulset pod name [{statefulSetPodName}]");
    }
}
