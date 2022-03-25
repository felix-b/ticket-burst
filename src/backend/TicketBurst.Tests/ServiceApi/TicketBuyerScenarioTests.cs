using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests.ServiceApi;

[TestFixture]
public class TicketBuyerScenarioTests
{
    [Test]
    public async Task CanBuyTickets()
    {
        var searchResults = (await ServiceClient.HttpGetJson<IEnumerable<EventSearchResultContract>>(
            ServiceName.Search, 
            path: new[] { "search" })
        )?.ToArray();

        searchResults.Should().NotBeNull();
        
        var @event = searchResults!.First();
        
        
        
       // Console.WriteLine(results?.Count() ?? -1);
    }
}
