using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Murmur;
using NUnit.Framework;

namespace TicketBurst.Tests.TryOut;

[TestFixture]
public class DotnetTests
{
    [Test]
    public void CanDeserializeJsonIntoCSharpRecord()
    {
        var json = "{\"username\":\"MY_USER\",\"host\":\"MY_HOST\",\"port\":3306}";

        var secret = JsonSerializer.Deserialize<MySecretRecord>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        secret.Should().NotBeNull();
        secret!.Host.Should().Be("MY_HOST");
        secret!.UserName.Should().Be("MY_USER");
        secret!.Port.Should().Be(3306);
    }

    //[Test]
    public void CanComputePartitionHashes()
    {
        var algorithm = MurmurHash.Create32();
        
        var json = File.ReadAllText(@"D:\\temp\\event-areas.json");
        var list = JsonSerializer.Deserialize<EventAreaList>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        UseHashFunction("string.GetHashCode", s => (uint)s.GetHashCode());
        UseHashFunction("darrenkopp/murmurhash-net(32-bit)", s => ComputeMurmurHash(s));

        uint ComputeMurmurHash(string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            byte[] hash = algorithm.ComputeHash(bytes);
            uint result = (uint)hash[0] | (uint)(hash[1] << 8) | (uint)(hash[2] << 16) | (uint)(hash[3] << 24);
            return result;
        }
        
        void UseHashFunction(string title, Func<string, uint> hashFunc)
        {
            var hashSet = new HashSet<uint>();
            var hashCollisions = new Dictionary<uint, uint>();
            var shardDistribution2 = new Dictionary<uint, uint>() {
                { 0, 0 }, { 1, 0 }
            };
            var shardDistribution3 = new Dictionary<uint, uint>() {
                { 0, 0 }, { 1, 0 }, { 2, 0 },
            };
            var shardDistribution4 = new Dictionary<uint, uint>() {
                { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 },
            };
            var shardDistribution5 = new Dictionary<uint, uint>() {
                { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 },
            };
            var shardDistribution10 = new Dictionary<uint, uint>() {
                { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 },
            };

            foreach (var item in list!.Data)
            {
                var hash = hashFunc(item.Id);//(uint)item.Id.GetHashCode();
                hashSet.Add(hash);
                if (hashCollisions.TryGetValue(hash, out var count))
                {
                    hashCollisions[hash] = count + 1;
                }
                else
                {
                    hashCollisions[hash] = 0;
                }

                shardDistribution2[(hash % 2)] = shardDistribution2[(hash % 2)] + 1;
                shardDistribution3[(hash % 3)] = shardDistribution3[(hash % 3)] + 1;
                shardDistribution4[(hash % 4)] = shardDistribution4[(hash % 4)] + 1;
                shardDistribution5[(hash % 5)] = shardDistribution5[(hash % 5)] + 1;
                shardDistribution10[(hash % 10)] = shardDistribution10[(hash % 10)] + 1;
            }

            Console.WriteLine($"items................. {list.Data.Length}");
            Console.WriteLine($"hash set size......... {hashSet.Count}");
            Console.WriteLine($"shard distribution/2.. {string.Join(' ', shardDistribution2.Values)}");
            Console.WriteLine($"shard distribution/3.. {string.Join(' ', shardDistribution3.Values)}");
            Console.WriteLine($"shard distribution/4.. {string.Join(' ', shardDistribution4.Values)}");
            Console.WriteLine($"shard distribution/5.. {string.Join(' ', shardDistribution5.Values)}");
            Console.WriteLine($"shard distribution/10. {string.Join(' ', shardDistribution10.Values)}");
            Console.WriteLine($"---------hash collisions--------");

            var collisionGroups = hashCollisions
                .Where(kvp => kvp.Value > 0)
                .GroupBy(kvp => kvp.Value);

            foreach (var group in collisionGroups)
            {
                Console.WriteLine($"[{group.Count()}] hashes resulted in [{group.Key}] collisions");
            }
        }
    }
    
    public record MySecretRecord(
        string UserName, 
        string Host, 
        int Port
    );

    public record EventAreaList(EventAreaItem[] Data);
    public record EventAreaItem(string Id, string Name);
}

