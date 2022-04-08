using System.Collections.Immutable;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations;

public class MongoDbReservationEntityRepository : IReservationEntityRepository
{
    private readonly IMongoDatabase _database; 
    private readonly IMongoCollection<ReservationJournalRecordForDb> _journal; 

    static MongoDbReservationEntityRepository()
    {
        var pack = new ConventionPack {
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register(nameof(CamelCaseElementNameConvention), pack, _ => true);        
    }
    
    public MongoDbReservationEntityRepository(ConnectionStringSecret secret)
    {
        var connectionString = !string.IsNullOrWhiteSpace(secret.Host)
            ? $"mongodb://{secret.UserName}:{secret.Password}@{secret.Host}:{secret.Port}/?replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false"
            : "mongodb://localhost";

        Console.WriteLine($"MongoDbReservationEntityRepository: using connection string [{connectionString}]");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("reservation_service");

        _journal = _database.GetCollection<ReservationJournalRecordForDb>("journal");
    }
    
    public string MakeNewId()
    {
        return ObjectId.GenerateNewId().ToString();
    }

    public IAsyncEnumerable<ReservationJournalRecord> GetJournalEntriesForRecovery(string eventId, string areaId)
    {
        var eventObjectId = new ObjectId(eventId);
        var areaObjectId = new ObjectId(areaId);

        var cursor = _journal.AsQueryable()
            .Where(r => r.EventId == eventObjectId && r.HallAreaId == areaObjectId)
            .OrderBy(r => r.SequenceNo)
            .ToCursor();
            
        return cursor.AsTranslatingAsyncEnumerable(recordForDb => recordForDb.ToImmutable());
    }

    public async Task AppendJournalEntry(ReservationJournalRecord record)
    {
        var recordForDb = new ReservationJournalRecordForDb(record);
        Console.WriteLine($"MongoDbReservationEntityRepository::AppendJournalEntry> {JsonSerializer.Serialize(recordForDb)}");
        await _journal.InsertOneAsync(recordForDb);
    }

    public class ReservationJournalRecordForDb
    {
        public ReservationJournalRecordForDb(ReservationJournalRecord source)
        {
            Id = new ObjectId(source.Id); 
            CreatedAtUtc = source.CreatedAtUtc; 
            EventId = new ObjectId(source.EventId); 
            HallAreaId = new ObjectId(source.HallAreaId); 
            HallSeatingMapId = new ObjectId(source.HallSeatingMapId); 
            SequenceNo = source.SequenceNo; 
            SeatIds = source.SeatIds.Select(id => new ObjectId(id)).ToList(); 
            Action = source.Action; 
            ResultStatus = (int)source.ResultStatus; 
            OrderNumber = source.OrderNumber; 
        }

        public ReservationJournalRecord ToImmutable()
        {
            return new ReservationJournalRecord(
                Id.ToString(),
                CreatedAtUtc,
                EventId.ToString(),
                HallAreaId.ToString(),
                HallSeatingMapId.ToString(),
                SequenceNo,
                SeatIds.Select(id => id.ToString()).ToImmutableList(),
                Action,
                (SeatStatus)ResultStatus,
                OrderNumber);
        }
        
        public ObjectId Id { get; set; } 
        public DateTime CreatedAtUtc { get; set; } 
        public ObjectId EventId { get; set; } 
        public ObjectId HallAreaId { get; set; } 
        public ObjectId HallSeatingMapId { get; set; } 
        public ulong SequenceNo { get; set; } 
        public List<ObjectId> SeatIds { get; set; } 
        public ReservationAction Action { get; set; } 
        public int ResultStatus { get; set; } 
        public uint? OrderNumber { get; set; } 
    }
}