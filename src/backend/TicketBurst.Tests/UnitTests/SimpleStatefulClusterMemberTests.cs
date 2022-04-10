#pragma warning disable CS1998
#pragma warning disable CS8602

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests.UnitTests;

[TestFixture]
public class SimpleStatefulClusterMemberTests
{
    [Test]
    public void InitialState_Steady()
    {
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        member.CurrentState.Should().NotBeNull();
        member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
        member.CurrentState.MemberCount.Should().Be(3);
        member.CurrentState.ThisMemberIndex.Should().Be(1);
        member.CurrentState.Generation.Should().Be(1);
        member.CurrentState.PendingRebalanceMemberCount.Should().BeNull();
    }

    [Test]
    public void CannotGetInitialInfo_Throw()
    {
        var infoProvider = new TestClusterInfoProvider(null);
        Assert.Throws<Exception>(() => new SimpleStatefulClusterMember(infoProvider, enablePolling: false));
    }

    [Test]
    public void SteadyState_MemberCountNotChanged_RemainSteady()
    {
        var changeCount = 0;
        Action onChange = () => changeCount++;

        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        member.Changed += onChange;
        member.CheckForChanges();

        member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
        changeCount.Should().Be(0);
    }

    [Test]
    public void SteadyState_MemberCountChanged_TransitionToWillRebalance()
    {
        var changeCount = 0;
        Action onChange = () => changeCount++;
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);

        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
        member.Changed += onChange;
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host")
        };
        member.CheckForChanges();

        changeCount.Should().Be(1);
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        member.CurrentState.MemberCount.Should().Be(3);
        member.CurrentState.MemberHostNames.Count.Should().Be(3);
        member.CurrentState.PendingRebalanceMemberCount.Should().Be(4);
    }

    [Test]
    public void WillRebalance_MemberCountNotChanged_TransitionToRebalanceLockdown()
    {
        var changeCount = 0;
        Action onChange = () => changeCount++;
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);

        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
        member.Changed += onChange;
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host")
        };
        member.CheckForChanges();

        changeCount.Should().Be(1);
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        
        member.CheckForChanges();

        changeCount.Should().Be(2);
        member.CurrentState.Status.Should().Be(ClusterStatus.RebalanceLockdown);
        member.CurrentState.MemberCount.Should().Be(4);
        member.CurrentState.MemberHostNames.Count.Should().Be(4);
        member.CurrentState.PendingRebalanceMemberCount.Should().BeNull();
    }

    [Test]
    public void WillRebalance_MemberCountChanged_TransitionToWillRebalance()
    {
        var changeCount = 0;
        Action onChange = () => changeCount++;
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);

        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
        member.Changed += onChange;
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-1")
        };
        member.CheckForChanges();

        changeCount.Should().Be(1);
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 5,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-2")
        };
        member.CheckForChanges();

        changeCount.Should().Be(2);
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        member.CurrentState.MemberCount.Should().Be(3);
        member.CurrentState.MemberHostNames.Count.Should().Be(3);
        member.CurrentState.PendingRebalanceMemberCount.Should().Be(5);
    }

    [Test]
    public void RebalanceLockdown_MemberCountNotChanged_TransitionToSteady()
    {
        var changeCount = 0;
        Action onChange = () => changeCount++;
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);

        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
        member.Changed += onChange;
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host")
        };
        member.CheckForChanges();
        member.CheckForChanges();

        changeCount.Should().Be(2);
        member.CurrentState.Status.Should().Be(ClusterStatus.RebalanceLockdown);
        
        member.CheckForChanges();

        changeCount.Should().Be(3);
        member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
        member.CurrentState.MemberCount.Should().Be(4);
        member.CurrentState.MemberHostNames.Count.Should().Be(4);
        member.CurrentState.PendingRebalanceMemberCount.Should().BeNull();
    }

    [Test]
    public void RebalanceLockdown_MemberCountChanged_TransitionToWillRebalance()
    {
        var changeCount = 0;
        Action onChange = () => changeCount++;
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);

        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
        member.Changed += onChange;
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-1")
        };
        member.CheckForChanges();
        member.CheckForChanges();

        changeCount.Should().Be(2);
        member.CurrentState.Status.Should().Be(ClusterStatus.RebalanceLockdown);
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 5,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-2")
        };
        member.CheckForChanges();

        changeCount.Should().Be(3);
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        member.CurrentState.MemberCount.Should().Be(4);
        member.CurrentState.MemberHostNames.Count.Should().Be(4);
        member.CurrentState.PendingRebalanceMemberCount.Should().Be(5);
    }

     [Test]
     public void SteadyState_CannotGetInfo_Ignore()
     {
         var changeCount = 0;
         Action onChange = () => changeCount++;
         var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
 
         var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
         member.Changed += onChange;
         
         infoProvider.Info = null;
         member.CheckForChanges();
 
         changeCount.Should().Be(0);
         member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
     }

     [Test]
     public void NonSteadyState_CannotGetInfoLessThan1Minute_Ignore()
     {
         var changeCount = 0;
         Action onChange = () => changeCount++;
         var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
 
         var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
         member.Changed += onChange;

         infoProvider.Info = infoProvider.Info with {
             MemberCount = 4,
             MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-1")
         };
         member.CheckForChanges();
         changeCount.Should().Be(1);
         member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
         
         infoProvider.Info = null;
         infoProvider.UtcNow += TimeSpan.FromSeconds(59);
         member.CheckForChanges();
 
         changeCount.Should().Be(1);
         member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
     }

     [Test]
     public void NonSteadyState_CannotGetInfoFor1Minute_RevertToSteady()
     {
         var changeCount = 0;
         Action onChange = () => changeCount++;
         var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
 
         var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);
         member.Changed += onChange;

         infoProvider.Info = infoProvider.Info with {
             MemberCount = 4,
             MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-1")
         };
         member.CheckForChanges();
         changeCount.Should().Be(1);
         member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
         
         infoProvider.Info = null;
         infoProvider.UtcNow += TimeSpan.FromSeconds(61);
         member.CheckForChanges();
 
         changeCount.Should().Be(2);
         member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
         member.CurrentState.MemberCount.Should().Be(3);
         member.CurrentState.MemberHostNames.Count.Should().Be(3);
         member.CurrentState.PendingRebalanceMemberCount.Should().BeNull();
     }

    [Test]
    public async Task WaitForSteadyState_Success()
    {
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-1")
        };
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);

        var clock = Stopwatch.StartNew();
        var waitForSteadyStateTask = member.WaitForSteadyStateOrThrow(TimeSpan.FromMinutes(1), CancellationToken.None);

        member.CheckForChanges();
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.Steady);

        await waitForSteadyStateTask;

        clock.ElapsedMilliseconds.Should().BeInRange(900, 2000);
    }

    [Test]
    public async Task WaitForSteadyState_Timeout_Throw()
    {
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        infoProvider.Info = infoProvider.Info with {
            MemberCount = 4,
            MemberHostNames = infoProvider.Info.MemberHostNames.Add("new-host-1")
        };
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);

        var clock = Stopwatch.StartNew();
        
        Assert.ThrowsAsync<TimeoutException>(async () => {
            await member.WaitForSteadyStateOrThrow(TimeSpan.FromMilliseconds(1500), CancellationToken.None);
        });

        clock.ElapsedMilliseconds.Should().BeInRange(900, 2500);
    }

    [Test]
    public void CanDistributeKeysIntoShards()
    {
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        var countByShard = new[] { 0, 0, 0 };

        for (int i = 0; i < 100; i++)
        {
            var key = $"111122223333{i,4:0000}";
            var shardIndex = member.GetShardMemberIndex(key);
            countByShard[shardIndex]++;

            if (countByShard[shardIndex] == 1)
            {
                Console.WriteLine($"Key [{key}] -> shard {shardIndex}");
            }
        }

        var clock = new Stopwatch();
        for (int i = 0; i < 100; i++)
        {
            var key = $"111122223333{i,4:0000}";
            clock.Start();
            var shardIndex = member.GetShardMemberIndex(key);
            clock.Stop();
            countByShard[shardIndex]++;
        }
        
        Console.WriteLine(string.Join(' ', countByShard));
        Console.WriteLine(clock.Elapsed);

        var imbalance = Math.Abs(countByShard.Max() - countByShard.Min());
        imbalance.Should().BeLessThan(50);
    }

    [Test]
    public async Task SteadyState_MessageBelongsToShard_HandleHere()
    {
        // Key [1111222233330000] -> shard 0
        // Key [1111222233330001] -> shard 1
        // Key [1111222233330006] -> shard 2
        
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        var key = "1111222233330001"; 
        member.GetShardMemberIndex(key).Should().Be(1);

        var handleHereCount = 0;
        var clock = Stopwatch.StartNew();
        
        await member.RouteMessageAsync(
            key,
            handleHere: () => handleHereCount++,
            relayToMember: (index) => Assert.Fail($"unexpected call relayToMember({index})"),
            awaitSteadyTimeout: TimeSpan.FromSeconds(1),
            CancellationToken.None);

        clock.ElapsedMilliseconds.Should().BeLessThan(500);
        handleHereCount.Should().Be(1);
    }
    
    [Test]
    public async Task SteadyState_MessageBelongsToDifferentShard_Relay()
    {
        // Key [1111222233330000] -> shard 0
        // Key [1111222233330001] -> shard 1
        // Key [1111222233330006] -> shard 2
        
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        var key = "1111222233330006"; 
        member.GetShardMemberIndex(key).Should().Be(2);

        var relayCount = 0;
        var clock = Stopwatch.StartNew();
        
        await member.RouteMessageAsync(
            key,
            handleHere: () => Assert.Fail($"unexpected call handleHere()"),
            relayToMember: (index) => {
                relayCount++;
                Assert.AreEqual(2, index);
            },
            awaitSteadyTimeout: TimeSpan.FromSeconds(1),
            CancellationToken.None
        );

        clock.ElapsedMilliseconds.Should().BeLessThan(500);
        relayCount.Should().Be(1);
    }

    [Test]
    public async Task Rebalancing_MessageBelongedToShardButNotInFuture_WaitForSteadyAndRelay()
    {
        // Key [1111222233330000] -> shard 0
        // Key [1111222233330001] -> shard 1
        // Key [1111222233330006] -> shard 2
        
        var infoProvider = new TestClusterInfoProvider(memberCount: 3, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        var key = "1111222233330001"; 
        member.GetShardMemberIndex(key).Should().Be(1);
        member.GetShardMemberIndex(key, whatIfMemberCount: 7).Should().Be(3);
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 7,
            MemberHostNames = infoProvider.Info.MemberHostNames.AddRange(new[] { 
                "new-host-1", "new-host-2", "new-host-3", "new-host-4"
            })
        };
        
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        
        var relayCount = 0;
        var clock = Stopwatch.StartNew();
        
        var routeTask = member.RouteMessageAsync(
            key,
            handleHere: () => Assert.Fail($"unexpected call handleHere()"),
            relayToMember: (index) => {
                relayCount++;
                Assert.AreEqual(3, index);
            },
            awaitSteadyTimeout: TimeSpan.FromSeconds(5),
            CancellationToken.None);

        await Task.Delay(500);
        
        member.CheckForChanges();
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
        
        await routeTask;
        
        clock.ElapsedMilliseconds.Should().BeInRange(900, 2000);
        relayCount.Should().Be(1);
    }

    [Test]
    public async Task Rebalancing_MessageDidNotBelongToShardButWillInFuture_WaitForSteadyAndHandleHere()
    {
        // Key [1111222233330000] -> shard 0
        // Key [1111222233330001] -> shard 1
        // Key [1111222233330006] -> shard 2
        
        var infoProvider = new TestClusterInfoProvider(memberCount: 7, thisMemberIndex: 1);
        var member = new SimpleStatefulClusterMember(infoProvider, enablePolling: false);

        var key = "1111222233330001"; 
        member.GetShardMemberIndex(key).Should().Be(3);
        member.GetShardMemberIndex(key, whatIfMemberCount: 3).Should().Be(1);
        
        infoProvider.Info = infoProvider.Info with {
            MemberCount = 3,
            MemberHostNames = infoProvider.Info.MemberHostNames.Take(3).ToImmutableList()
        };
        
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.WillRebalance);
        
        var handleHereCount = 0;
        var clock = Stopwatch.StartNew();
        
        var routeTask = member.RouteMessageAsync(
            key,
            handleHere: () => handleHereCount++,
            relayToMember: (index) => Assert.Fail($"unexpected call relayToMember({index})"),
            awaitSteadyTimeout: TimeSpan.FromSeconds(5),
            CancellationToken.None);

        await Task.Delay(500);
        
        member.CheckForChanges();
        member.CheckForChanges();
        member.CurrentState.Status.Should().Be(ClusterStatus.Steady);
        
        await routeTask;
        
        clock.ElapsedMilliseconds.Should().BeInRange(900, 2000);
        handleHereCount.Should().Be(1);
    }
    
    private class TestClusterInfoProvider : IClusterInfoProvider
    {
        public TestClusterInfoProvider(ClusterInfo? info)
        {
            Info = info;
        }

        public TestClusterInfoProvider(int memberCount, int thisMemberIndex)
        {
            Info = new ClusterInfo(
                MemberCount: memberCount,
                ThisMemberIndex: thisMemberIndex,
                Generation: 1,
                MemberHostNames: Enumerable.Range(0, memberCount).Select(i => $"host-{i}").ToImmutableList()
            );
        }

        public void Dispose()
        {
        }

        public string GetEndpointUrl(string memberHostName, int memberIndex)
        {
            throw new NotImplementedException();
        }
        
        ClusterInfo? IClusterInfoProvider.TryGetInfo() => Info;
        
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;

        public ClusterInfo? Info { get; set; }
    }
}