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
    private readonly System.Threading.Timer _timer;

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
            
        Console.WriteLine($"> querying stateful set [{_statefulSetName}]");
                
        var statefulSet = _client.ReadNamespacedStatefulSet(_statefulSetName, _namespaceName);
        Console.WriteLine($"> {(statefulSet != null ? "success" : "FAILURE")}");

        if (statefulSet != null)
        {
            PrintStatefulSet(statefulSet);
            Current = CreateInfo(statefulSet);
        }
        else
        {
            throw new Exception("K8sClusterInfoProvider could not retrieve statefulset configuration");
        }

        _timer = new Timer(CheckForChanges, null, dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    public async Task WaitForSteadyStateOrThrow(TimeSpan timeout)
    {
        var timeoutAtUtc = DateTime.UtcNow.Add(timeout);

        while (Current.Status != ClusterStatus.Steady)
        {
            if (DateTime.UtcNow >= timeoutAtUtc)
            {
                throw new TimeoutException($"Timed out waiting for cluster steady state");
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    public ClusterInfo Current { get; private set; } = ClusterInfo.Empty;
    
    public event Action? Changed;

    private void SetCurrent(V1StatefulSet statefulSet, ClusterInfo newInfo)
    {
        ValidateOrFailFast();
        Current = newInfo;

        PrintStatefulSet(statefulSet);
        PrintClusterInfo(newInfo);
        
        try
        {
            Changed?.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        void ValidateOrFailFast()
        {
            if (newInfo.ThisMemberIndex < 0 || newInfo.ThisMemberIndex >= newInfo.MemberCount)
            {
                Console.WriteLine(
                    $"K8sClusterInfoProvider> PANIC> Got ClusterInfo.MemberCount={newInfo.MemberCount}, ClusterInfo.ThisMemberIndex={newInfo.ThisMemberIndex}");
                Console.WriteLine(
                    $"K8sClusterInfoProvider> FAILING FAST!");
                Environment.Exit(255);
            }
        }
    }
    
    private void CheckForChanges(object state)
    {
        try
        {
            var statefulSet = _client.ReadNamespacedStatefulSet(_statefulSetName, _namespaceName);
            if (statefulSet.Spec.Replicas.HasValue)
            {
                Console.WriteLine($"K8sClusterInfoProvider> checking for changes, last known replica count [{statefulSet.Spec.Replicas}]");
                ApplyToStateMachine(statefulSet);
            }
            else
            {
                Console.WriteLine($"K8sClusterInfoProvider> WARNING: cannot check for changes! spec.replicas = null");
                if (Current.Status != ClusterStatus.Steady && Current.StatusDuration.TotalMinutes >= 1)
                {
                    Console.WriteLine($"K8sClusterInfoProvider> WARNING: too long not in steady state and cannot check for changes - reverting to steady");
                    SetCurrent(statefulSet, Current with {
                        Status = ClusterStatus.Steady,
                        PendingRebalanceMemberCount = null,
                        SinceUtc = DateTime.UtcNow,
                    });
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"K8sClusterInfoProvider> FAILED to check for changes! {e.ToString()}");
        }

        void ApplyToStateMachine(V1StatefulSet mostRecent)
        {
            int lastKnownReplicaCount = mostRecent.Spec.Replicas!.Value;
            
            switch (Current.Status)
            {
                case ClusterStatus.Steady:
                    if (Current.MemberCount != lastKnownReplicaCount)
                    {
                        SetCurrent(mostRecent, Current with {
                            Status = ClusterStatus.WillRebalance,
                            PendingRebalanceMemberCount = lastKnownReplicaCount,
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    break;
                case ClusterStatus.WillRebalance:
                    if (Current.PendingRebalanceMemberCount == lastKnownReplicaCount)
                    {
                        SetCurrent(mostRecent, Current with {
                            Status = ClusterStatus.RebalanceLockdown,
                            MemberCount = lastKnownReplicaCount, 
                            PendingRebalanceMemberCount = null,
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        SetCurrent(mostRecent, Current with {
                            Status = ClusterStatus.WillRebalance,
                            PendingRebalanceMemberCount = lastKnownReplicaCount, 
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    break;
                case ClusterStatus.RebalanceLockdown:
                    if (Current.PendingRebalanceMemberCount == lastKnownReplicaCount)
                    {
                        SetCurrent(mostRecent, Current with {
                            Status = ClusterStatus.Steady,
                            MemberCount = lastKnownReplicaCount, 
                            PendingRebalanceMemberCount = null,
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        SetCurrent(mostRecent, Current with {
                            Status = ClusterStatus.WillRebalance,
                            PendingRebalanceMemberCount = lastKnownReplicaCount, 
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    break;
            }
        }
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

    private void PrintClusterInfo(ClusterInfo info)
    {
        Console.WriteLine($"------ClusterInfo------");
        Console.WriteLine($"> info.Status={info.Status}");
        Console.WriteLine($"> info.StatusDuration={info.StatusDuration}");
        Console.WriteLine($"> info.Generation={info.Generation}");
        Console.WriteLine($"> info.MemberCount={info.MemberCount}");
        Console.WriteLine($"> info.ThisMemberIndex={info.ThisMemberIndex}");
        Console.WriteLine($"> info.PendingRebalanceMemberCount={info.PendingRebalanceMemberCount}");
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
            Generation: statefulSet.Metadata.Generation.GetValueOrDefault(-1),
            Status: ClusterStatus.Steady,
            SinceUtc: DateTime.UtcNow
        );
    }

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
            Status: ClusterStatus.Steady,
            MemberHostNames: podsAlive.Select(p => $"{p.Metadata.Name}.{_namespaceName}").ToImmutableList(),
            SinceUtc: DateTime.UtcNow,
            PendingRebalanceMemberCount: null
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
