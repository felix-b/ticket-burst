using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using NUnit.Framework;
using TicketBurst.Reservation.Integrations.SimpleSharding;

namespace TicketBurst.Tests.ServiceApi;

[TestFixture]
public class ShardClientTests
{
    [Test]
    public async Task CanPingActorInShard()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:8090");
        var client = new EventAreaManagerShard.EventAreaManagerShardClient(channel);

        var response = await client.PingAsync(new PingRequest());
        response.Should().NotBeNull();
    }
}
