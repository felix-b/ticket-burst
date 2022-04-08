﻿using System.Collections.Immutable;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Partition;
using Proto.Cluster.Seed;
using Proto.Remote;
using Proto.Remote.GrpcNet;
using Proto.Router;
using TicketBurst.Reservation.ProtoActor;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Contracts;
using ProtosReflection = Proto.Remote.ProtosReflection;

namespace TicketBurst.ReservationService.Integrations.ProtoActor;

public class ProtoActorEngine : IActorEngine
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ActorSystem _actorSystem;
    private readonly object _localGrainsSyncRoot = new();
    private ImmutableHashSet<EventAreaManagerGrain> _localGrains;

    public ProtoActorEngine(IServiceProvider serviceProvider, int listenPort = 0)
    {
        _serviceProvider = serviceProvider;
        _localGrains = ImmutableHashSet<EventAreaManagerGrain>.Empty;
        
        Proto.Log.SetLoggerFactory(LoggerFactory.Create(l => l.AddConsole().SetMinimumLevel(LogLevel.Debug)));
        
        // Required to allow unencrypted GrpcNet connections
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        _actorSystem = new ActorSystem(new ActorSystemConfig().WithDeveloperSupervisionLogging(true))
            .WithRemote(GrpcNetRemoteConfig.BindToLocalhost(listenPort).WithProtoMessages(ProtosReflection.Descriptor))
            .WithCluster(ClusterConfig
                .Setup(
                    clusterName: "TicketBurstReservationCluster", 
                    clusterProvider: new SeedNodeClusterProvider(
                        listenPort == 0
                            ? new SeedNodeClusterProviderOptions(("127.0.0.1", 8080))
                            : null
                    ), 
                    identityLookup: new PartitionIdentityLookup()
                )
                .WithClusterKind(
                    EventAreaManagerGrainActor.GetClusterKind(
                        innerFactory: (ctx, identity) => new EventAreaManagerGrain(
                            ctx, 
                            identity.Identity, 
                            actorEngine:this, 
                            serviceProvider
                        )
                    )
                )
            );
        
        _actorSystem.EventStream.Subscribe<ClusterTopology>(e => {
                Console.WriteLine($"{DateTime.Now:O} My members {e.TopologyHash}");
            }
        );
    }

    public Task StartAsync()
    {
        return _actorSystem
            .Cluster()
            .StartMemberAsync();
    }
    
    public ValueTask DisposeAsync()
    {
        return _actorSystem.DisposeAsync();
    }

    public Task<IEventAreaManager?> GetActor(string eventId, string areaId)
    {
        var identity = EventAreaManagerGrainIdentity.Construct(eventId, areaId);
        var client = _actorSystem.Cluster().GetEventAreaManagerGrain(identity);
        var proxy = new EventAreaManagerProxy(eventId, areaId, client);
        
        return Task.FromResult<IEventAreaManager?>(proxy);
    }

    public async Task ForEachLocalActor(Func<IEventAreaManager, Task> action)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        var localGrainsSnapshot = _localGrains;
        
        foreach (var grain in localGrainsSnapshot)
        {
            var proxy = grain.GetProxyOfSelf();
            
            try
            {
                await action(proxy);
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAILURE in EAM[{proxy.EventId}/{proxy.AreaId}]: {e.ToString()}");
            }
        }
    }

    public void RegisterLocalGrain(EventAreaManagerGrain grain)
    {
        lock (_localGrainsSyncRoot)
        {
            _localGrains = _localGrains.Add(grain);
        }
    }

    public void UnRegisterLocalGrain(EventAreaManagerGrain grain)
    {
        lock (_localGrainsSyncRoot)
        {
            _localGrains = _localGrains.Remove(grain);
        }
    }

    public ClusterDiagnosticInfo GetClusterDiagnostics()
    {
        var cluster = _actorSystem.Cluster();
        return new ClusterDiagnosticInfo(
            Address: _actorSystem.Address,
            ClusterName: cluster.Config.ClusterName,
            ThisMember: GetMemberDiagnostics(cluster.MemberList.Self),
            OtherMembers: cluster.MemberList.GetOtherMembers().Select(GetMemberDiagnostics).ToArray(),
            LocalGrains: _localGrains.Select(g => g.Identity).ToArray()
        );
    }

    public Cluster Cluster => _actorSystem.Cluster();
    
    private MemberDiagnosticInfo GetMemberDiagnostics(Member  source)
    {
        return new MemberDiagnosticInfo(Id: source.Id, Address: source.Address, Host: source.Host, Port: source.Port);
    }
}
