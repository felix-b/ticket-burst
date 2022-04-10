using System.Collections.Immutable;
using Grpc.Net.Client;
using Microsoft.Extensions.ObjectPool;
using TicketBurst.Reservation.Integrations.SimpleSharding;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations.SimpleSharding;

public class EventAreaManagerShardClientPool : IDisposable
{
    private readonly SimpleStatefulClusterMember _cluster;
    private readonly object _poolByMemberHostNameSyncRoot = new();
    private ImmutableDictionary<int, GrpcChannel> _channelByMemberIndex;
    private bool _isDisposed = false;

    public EventAreaManagerShardClientPool(SimpleStatefulClusterMember cluster)
    {
        _cluster = cluster;
        _channelByMemberIndex = ImmutableDictionary<int, GrpcChannel>.Empty;

        _cluster.Changed += OnClusterChanged;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(EventAreaManagerShardClientPool));
        }

        _cluster.Changed -= OnClusterChanged;

        foreach (var channel in _channelByMemberIndex.Values)
        {
            channel.Dispose();
        }
    }

    public EventAreaManagerShard.EventAreaManagerShardClient GetClient(int memberIndex)
    {
        var channel = GetChannelToMember(memberIndex);
        return new EventAreaManagerShard.EventAreaManagerShardClient(channel);
    }

    private GrpcChannel GetChannelToMember(int memberIndex)
    {
        var state = _cluster.CurrentState;
        if (memberIndex >= state.MemberCount)
        {
            throw new IndexOutOfRangeException(
                $"Attempt to get member index [{memberIndex}] whereas cluster only has [{state.MemberCount}] members");
        }

        if (!_channelByMemberIndex.TryGetValue(memberIndex, out var channel))
        {
            lock (_poolByMemberHostNameSyncRoot)
            {
                if (!_channelByMemberIndex.TryGetValue(memberIndex, out channel))
                {
                    var hostName = state.MemberHostNames[memberIndex];
                    var endpointUrl = _cluster.InfoProvider.GetEndpointUrl(hostName, memberIndex);
                    channel = GrpcChannel.ForAddress(endpointUrl);
                    _channelByMemberIndex = _channelByMemberIndex.Add(memberIndex, channel);
                }
            }
        }

        return channel;
    }
    
    private void OnClusterChanged()
    {
        try
        {
            var state = _cluster.CurrentState;
            if (state.Status == ClusterStatus.Steady)
            {
                RecycleAllChannels();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"EventAreaManagerShardClientPool.OnClusterChanged FAILED! {e.ToString()}");
        }        
    }

    private void RecycleAllChannels()
    {
        GrpcChannel[] channelsToDispose;
            
        lock (_poolByMemberHostNameSyncRoot)
        {
            channelsToDispose = _channelByMemberIndex.Values.ToArray();
            _channelByMemberIndex = ImmutableDictionary<int, GrpcChannel>.Empty;
        }

        foreach (var channel in channelsToDispose)
        {
            channel.Dispose();
        }
    }
}
