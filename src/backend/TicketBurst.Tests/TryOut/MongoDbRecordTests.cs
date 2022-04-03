using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests.TryOut;

[TestFixture(Category = "tryout")]
public class MongoDbRecordTests
{
    private IMongoCollection<MyRecord>? _collection = null;

    [SetUp]
    public void BeforeEach()
    {
        var connectionString = "mongodb://localhost";
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase("test");
        _collection = db.GetCollection<MyRecord>("my_records");
        _collection.DeleteMany(r => true);
    }
    
    [Test]
    public async Task TestIdCreationInRecords()
    {
        var id = ObjectId.GenerateNewId();
        var record1 = new MyRecord(id.ToString(), "ABC", 123);
        await _collection!.InsertOneAsync(record1);

        var record2 = await _collection.Find(r => r.Id == id.ToString()).SingleAsync();

        record2.Should().NotBeNull();
        record2.Id.Should().Be(id.ToString());
        record2.Name.Should().Be("ABC");
        record2.Value.Should().Be(123);
        
        Console.WriteLine(id.ToString());
    }

    [Test]
    public async Task TestAsyncCursor()
    {
        var record1 = new MyRecord(ObjectId.GenerateNewId().ToString(), "ABC", 123);
        var record2 = new MyRecord(ObjectId.GenerateNewId().ToString(), "DEF", 456);
        var record3 = new MyRecord(ObjectId.GenerateNewId().ToString(), "GHI", 789);

        await _collection!.InsertManyAsync(new[] { record1, record2, record3 });

        var options = new FindOptions<MyRecord>();
        options.Sort.Ascending(r => r.Name);
        var cursor = await _collection.FindAsync(r => true, options);
        var results = await cursor.ToListAsync();

        results.Count.Should().Be(3);
        results[0].Id.Should().Be(record1.Id);
        results[1].Id.Should().Be(record2.Id);
        results[2].Id.Should().Be(record3.Id);
    }

    [Test]
    public async Task TestAsAsyncEnumerable()
    {
        var record1 = new MyRecord(ObjectId.GenerateNewId().ToString(), "ABC", 123);
        var record2 = new MyRecord(ObjectId.GenerateNewId().ToString(), "DEF", 456);
        var record3 = new MyRecord(ObjectId.GenerateNewId().ToString(), "GHI", 789);

        await _collection!.InsertManyAsync(new[] { record1, record2, record3 });

        var options = new FindOptions<MyRecord>();
        options.Sort.Ascending(r => r.Name);
        var cursor = await _collection.FindAsync(r => true, options);
        var results = new List<MyRecord>();

        await foreach (var item in cursor.AsAsyncEnumerable())
        {
            results.Add(item);
        }

        results.Count.Should().Be(3);
        results[0].Id.Should().Be(record1.Id);
        results[1].Id.Should().Be(record2.Id);
        results[2].Id.Should().Be(record3.Id);
    }

    [Test]
    public void TestToListSync()
    {
        var record1 = new MyRecord(ObjectId.GenerateNewId().ToString(), "ABC", 123);
        var record2 = new MyRecord(ObjectId.GenerateNewId().ToString(), "DEF", 456);
        var record3 = new MyRecord(ObjectId.GenerateNewId().ToString(), "GHI", 789);

        _collection!.InsertMany(new[] { record1, record2, record3 });

        var options = new FindOptions<MyRecord>();
        options.Sort.Ascending(r => r.Name);
        
        var cursorTask = _collection.FindAsync(r => true, options);
        cursorTask.Wait();
        var cursor = cursorTask.Result;
        var results = cursor.AsAsyncEnumerable().ToListSync();

        results.Count.Should().Be(3);
        results[0].Id.Should().Be(record1.Id);
        results[1].Id.Should().Be(record2.Id);
        results[2].Id.Should().Be(record3.Id);
    }

    public record MyRecord(string Id, string Name, int Value);
}

