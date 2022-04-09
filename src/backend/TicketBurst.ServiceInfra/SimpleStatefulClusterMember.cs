using System.Text;
using Murmur;

namespace TicketBurst.ServiceInfra;

public class SimpleStatefulClusterMember : IDisposable
{
    private readonly IClusterInfoProvider _infoProvider;
    private readonly System.Threading.Timer? _pollingTimer;

    public SimpleStatefulClusterMember(IClusterInfoProvider infoProvider, bool enablePolling)
    {
        Console.WriteLine($"SimpleStatefulClusterMember> initializing.");

        _infoProvider = infoProvider;

        var info = _infoProvider.GetInfoOrThrow();
        CurrentState = CreateStateFromInfo(info);

        _pollingTimer = enablePolling
            ? new Timer(_ => CheckForChanges(), null, dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10))
            : null;
        
        PrintClusterState(CurrentState);
    }

    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }

    public async Task WaitForSteadyStateOrThrow(TimeSpan timeout, CancellationToken cancellation)
    {
        var timeoutAtUtc = DateTime.UtcNow.Add(timeout);

        while (CurrentState.Status != ClusterStatus.Steady && !cancellation.IsCancellationRequested)
        {
            if (DateTime.UtcNow >= timeoutAtUtc)
            {
                throw new TimeoutException($"Timed out waiting for cluster steady state");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellation);
        }

        cancellation.ThrowIfCancellationRequested();
    }

    public void CheckForChanges()
    {
        try
        {
            var info = _infoProvider.TryGetInfo();
            if (info != null)
            {
                Console.WriteLine($"SimpleStatefulClusterMember> checking for changes, got member count [{info.MemberCount}]");
                ApplyToStateMachine(info);
            }
            else
            {
                var stateDuration = CurrentState.GetStatusDuration(_infoProvider.UtcNow);
                Console.WriteLine($"SimpleStatefulClusterMember> WARNING: cannot check for changes! {_infoProvider.GetType().Name} returned null");

                if (CurrentState.Status != ClusterStatus.Steady && stateDuration.TotalMinutes >= 1)
                {
                    Console.WriteLine($"SimpleStatefulClusterMember> WARNING: too long not in steady state and cannot check for changes - reverting to steady");
                    SetCurrent(CurrentState with {
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

        void ApplyToStateMachine(ClusterInfo info)
        {
            int mostRecentMemberCount = info.MemberCount;
            
            switch (CurrentState.Status)
            {
                case ClusterStatus.Steady:
                    if (mostRecentMemberCount != CurrentState.MemberCount)
                    {
                        SetCurrent(CurrentState with {
                            Status = ClusterStatus.WillRebalance,
                            PendingRebalanceMemberCount = mostRecentMemberCount,
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    break;
                case ClusterStatus.WillRebalance:
                    if (mostRecentMemberCount == CurrentState.PendingRebalanceMemberCount)
                    {
                        SetCurrent(CurrentState with {
                            Status = ClusterStatus.RebalanceLockdown,
                            MemberCount = mostRecentMemberCount,
                            MemberHostNames = info.MemberHostNames,
                            PendingRebalanceMemberCount = null,
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        SetCurrent(CurrentState with {
                            Status = ClusterStatus.WillRebalance,
                            PendingRebalanceMemberCount = mostRecentMemberCount, 
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    break;
                case ClusterStatus.RebalanceLockdown:
                    if (mostRecentMemberCount == CurrentState.MemberCount)
                    {
                        SetCurrent(CurrentState with {
                            Status = ClusterStatus.Steady,
                            PendingRebalanceMemberCount = null,
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        SetCurrent(CurrentState with {
                            Status = ClusterStatus.WillRebalance,
                            PendingRebalanceMemberCount = mostRecentMemberCount, 
                            SinceUtc = DateTime.UtcNow
                        });
                    }
                    break;
            }
        }
    }

    public uint GetShardMemberIndex(string key, int? whatIfMemberCount = null)
    {
        var effectiveMemberCount = whatIfMemberCount ?? CurrentState.MemberCount; 
        var hash = ComputeMurmur3Hash();
        return hash % (uint)effectiveMemberCount;
        
        uint ComputeMurmur3Hash()
        {
            var murmur3A = MurmurHash.Create32();
            var bytes = Encoding.ASCII.GetBytes(key);
            byte[] hash = murmur3A.ComputeHash(bytes);
            uint result = (uint)hash[0] | (uint)(hash[1] << 8) | (uint)(hash[2] << 16) | (uint)(hash[3] << 24);
            return result;
        }
    }

    public async Task RouteMessageAsync(
        string key, 
        Action handleHere, 
        Action<uint> relayToMember, 
        TimeSpan awaitSteadyTimeout, 
        CancellationToken cancellation)
    {
        var state = CurrentState;
        
        var currentKeyMemberIndex = GetShardMemberIndex(key, state.MemberCount);
        var futureKeyMemberIndex = state.PendingRebalanceMemberCount.HasValue
            ? GetShardMemberIndex(key, whatIfMemberCount: state.PendingRebalanceMemberCount.Value)
            : currentKeyMemberIndex;

        if (currentKeyMemberIndex == futureKeyMemberIndex)
        {
            if (currentKeyMemberIndex == state.ThisMemberIndex)
            {
                handleHere();
            }
            else
            {
                relayToMember(currentKeyMemberIndex);
            }
        }
        else
        {
            await WaitForSteadyStateOrThrow(awaitSteadyTimeout, cancellation);
            
            // use CurrentState.MemberCount which might have changed while we were waiting
            var newKeyMemberIndex = GetShardMemberIndex(key, null); 
            
            if (newKeyMemberIndex == state.ThisMemberIndex)
            {
                handleHere();
            }
            else
            {
                relayToMember(newKeyMemberIndex);
            }
        }
    }

    public SimpleStatefulClusterState CurrentState { get; private set; }
    
    public event Action? Changed;

    private void SetCurrent(SimpleStatefulClusterState newState)
    {
        ValidateOrFailFast();
        CurrentState = newState;

        PrintClusterState(newState);
        
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
            if (newState.ThisMemberIndex < 0 || newState.ThisMemberIndex >= newState.MemberCount)
            {
                Console.WriteLine(
                    $"SimpleStatefulClusterMember> PANIC> ThisMemberIndex={newState.ThisMemberIndex} MemberCount={newState.MemberCount}");
                Console.WriteLine(
                    $"SimpleStatefulClusterMember> FAILING FAST!");
                Environment.Exit(255);
            }
        }
    }
    
    private void PrintClusterState(SimpleStatefulClusterState info)
    {
        Console.WriteLine($"------ClusterInfo------");
        Console.WriteLine($"> info.Status={info.Status}");
        Console.WriteLine($"> info.StatusDuration={info.GetStatusDuration(_infoProvider.UtcNow)}");
        Console.WriteLine($"> info.Generation={info.Generation}");
        Console.WriteLine($"> info.MemberCount={info.MemberCount}");
        Console.WriteLine($"> info.ThisMemberIndex={info.ThisMemberIndex}");
        Console.WriteLine($"> info.PendingRebalanceMemberCount={info.PendingRebalanceMemberCount}");
    }

    private SimpleStatefulClusterState CreateStateFromInfo(ClusterInfo info)
    {
        return new SimpleStatefulClusterState(
            MemberCount: info.MemberCount,
            ThisMemberIndex: info.ThisMemberIndex,
            MemberHostNames: info.MemberHostNames,
            Generation: info.Generation,
            Status: ClusterStatus.Steady,
            SinceUtc: DateTime.UtcNow
        );
    }
}