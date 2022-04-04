#pragma warning disable CS1998

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests.UnitTests;

[TestFixture]
public class ReadThroughCacheTests
{
    [Test]
    public async Task CanGetAndSet()
    {
        int loadCount = 0;
        Func<int, Task<string?>> loadValue = key => {
            loadCount++;
            return Task.FromResult<string?>($"{key}-loaded");
        }; 

        var cache = new ReadThroughCache<int, string>("test", loadValue);
        
        cache.Set(123, "123-assigned");
        cache.Set(789, "789-assigned");

        var value123 = await cache.Get(123);
        var value456 = await cache.Get(456);
        var value789 = await cache.Get(789);

        value123.Should().Be("123-assigned");
        value456.Should().Be("456-loaded");
        value789.Should().Be("789-assigned");
        
        loadCount.Should().Be(1);
    }
    
    [Test]
    public async Task MultipleRequests_SameValue()
    {
        int loadCount = 0;
        Func<int, Task<string?>> loadValue = key => {
            loadCount++;
            return Task.FromResult<string?>($"{key}-loaded");
        }; 
        
        var cache = new ReadThroughCache<int, string>("test", loadValue);
        
        cache.Set(123, "123-assigned");
        cache.Set(789, "789-assigned");

        var value123A = await cache.Get(123);
        var value456A = await cache.Get(456);
        var value789A = await cache.Get(789);
        
        var value123B = await cache.Get(123);
        var value456B = await cache.Get(456);
        var value789B = await cache.Get(789);

        value123B.Should().BeSameAs(value123A);
        value456B.Should().BeSameAs(value456A);
        value789B.Should().BeSameAs(value789A);

        loadCount.Should().Be(1);
    }

    [Test]
    public async Task MultipleSimultaneousRequesters_LoadOnce()
    {
        var loadStarted = new TaskCompletionSource();
        var secondRequestDone = new TaskCompletionSource();
        int loadCount = 0;
        
        Func<int, Task<string?>> loadValue = async key => {
            Interlocked.Increment(ref loadCount);
            loadStarted.SetResult();
            await secondRequestDone.Task;
            return $"{key}-loaded";
        };

        var cache = new ReadThroughCache<int, string>("test", loadValue);

        var firstRequestTask = DoFirstRequest();
        var secondRequestTask = DoSecondRequest();

        var firstValue = await firstRequestTask; 
        var secondValue = await secondRequestTask;

        firstValue.Should().Be("123-loaded");
        secondValue.Should().BeSameAs(firstValue);
        loadCount.Should().Be(1);

        async Task<string?> DoFirstRequest()
        {
            await Task.Yield();
            return await cache.Get(123);
        }

        async Task<string?> DoSecondRequest()
        {
            await Task.Yield();
            await loadStarted.Task;
            var promise = cache.Get(123);
            secondRequestDone.SetResult();
            return await promise;
        }
    }
}
