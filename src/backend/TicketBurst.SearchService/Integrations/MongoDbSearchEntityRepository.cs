#pragma warning disable CS1998
#pragma warning disable CS8618

using System.Collections.Immutable;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Integrations;

public class MongoDbSearchEntityRepository : ISearchEntityRepository
{
    private readonly IMongoDatabase _database; 
    private readonly IMongoCollection<EventContractForDb> _events; 
    private readonly IMongoCollection<ShowContractForDb> _shows; 
    private readonly IMongoCollection<ShowTypeContractForDb> _showTypes; 
    private readonly IMongoCollection<GenreContractForDb> _genres; 
    private readonly IMongoCollection<TroupeContractForDb> _troupes; 
    private readonly IMongoCollection<VenueContractForDb> _venues; 
    private readonly IMongoCollection<HallSeatingMapContractForDb> _hallSeatingMaps;

    static MongoDbSearchEntityRepository()
    {
        var pack = new ConventionPack {
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register(nameof(CamelCaseElementNameConvention), pack, _ => true);        
    }
    
    public MongoDbSearchEntityRepository(ConnectionStringSecret secret)
    {
        var connectionString = !string.IsNullOrWhiteSpace(secret.Host)
            ? $"mongodb://{secret.UserName}:{secret.Password}@{secret.Host}:{secret.Port}/?replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false"
            : "mongodb://localhost";
        
        Console.WriteLine($"MongoDbSearchEntityRepository: using connection string [{connectionString}]");
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("search_service");

        _events = _database.GetCollection<EventContractForDb>("events");
        _shows = _database.GetCollection<ShowContractForDb>("shows"); 
        _showTypes = _database.GetCollection<ShowTypeContractForDb>("showTypes"); 
        _genres = _database.GetCollection<GenreContractForDb>("genres"); 
        _troupes = _database.GetCollection<TroupeContractForDb>("troupes"); 
        _venues = _database.GetCollection<VenueContractForDb>("venues"); 
        _hallSeatingMaps = _database.GetCollection<HallSeatingMapContractForDb>("hall_seating_maps");
    }

    public string MakeNewId()
    {
        return ObjectId.GenerateNewId().ToString();
    }

    public async Task<IAsyncEnumerable<EventContract>> GetAllEvents()
    {
        var results = await _events.FindAsync(e => true);
        return results.AsTranslatingAsyncEnumerable(e => e.ToImmutable());
    }

    public async Task<IAsyncEnumerable<EventContract>> QueryEvents(EventSearchRequestContract query)
    {
        IQueryable<EventContractForDb> source = _events.AsQueryable();
        ApplyQuery();

        var results = source.ToArray();
        var translatedResults = results.Select(r => r.ToImmutable());
        return translatedResults.ToAsyncEnumerable();
        
        void ApplyQuery()
        {
            if (query.Selling == true)
            {
                source = source.OrderByDescending(e => e.IsOpenForSale);
            }

            if (!string.IsNullOrWhiteSpace(query.Id))
            {
                source = source.Where(e => e.Id == new ObjectId(query.Id));
            }

            if (query.FromDate.HasValue)
            {
                source = source.Where(e => e.EventStartUtc >= query.FromDate.Value.Date);
            }

            if (query.ToDate.HasValue)
            {
                source = source.Where(e => e.EventStartUtc < query.ToDate.Value.Date.AddDays(1));
            }

            source = source.OrderBy(e => e.EventStartUtc);
        }
    }

    public async Task<EventContract?> TryGetEventById(string eventId)
    {
        var results = await _events.FindAsync(e => e.Id == new ObjectId(eventId));
        return (await results.FirstOrDefaultAsync()).ToImmutable();
    }

    public async Task UpdateIsOpenForSale(string eventId, bool newValue)
    {
        var update = new BsonDocumentUpdateDefinition<EventContractForDb>(
            new BsonDocument(
                new BsonElement("$set", new BsonDocument(
                    new BsonElement("isOpenForSale", newValue)
                ))
            )
        );

        var result = await _events.UpdateOneAsync(e => e.Id == new ObjectId(eventId), update);

        Console.WriteLine(
            $"MongoDbSearchEntityRepository.UpdateIsOpenForSale [{eventId}]: modifiedCount=[{(result.IsModifiedCountAvailable ? result.ModifiedCount : -1)}], acknowledged=[{result.IsAcknowledged}]");
    }

    public async Task<IAsyncEnumerable<GenreContract>> GetAllGenres()
    {
        var results = await _genres.FindAsync(g => true);
        return results.AsTranslatingAsyncEnumerable(e => e.ToImmutable());
    }

    public async Task<GenreContract?> TryGetGenreById(string genreId)
    {
        var results = await _genres.FindAsync(g => g.Id == new ObjectId(genreId));
        return (await results.FirstOrDefaultAsync())?.ToImmutable();
    }

    public async Task<IAsyncEnumerable<ShowTypeContract>> GetAllShowTypes()
    {
        var results = await _showTypes.FindAsync(e => true);
        return results.AsTranslatingAsyncEnumerable(e => e.ToImmutable());
    }

    public async Task<ShowTypeContract?> TryGetShowTypeById(string showTypeId)
    {
        var results = await _showTypes.FindAsync(s => s.Id == new ObjectId(showTypeId));
        return (await results.FirstOrDefaultAsync())?.ToImmutable();
    }

    public async Task<IAsyncEnumerable<ShowContract>> GetAllShows()
    {
        var results = await _shows.FindAsync(e => true);
        return results.AsTranslatingAsyncEnumerable(e => e.ToImmutable());
    }

    public async Task<ShowContract?> TryGetShowById(string showId)
    {
        var results = await _shows.FindAsync(s => s.Id == new ObjectId(showId));
        return (await results.FirstOrDefaultAsync())?.ToImmutable();
    }

    public async Task<IAsyncEnumerable<TroupeContract>> GetAllTroupes()
    {
        var results = await _troupes.FindAsync(e => true);
        return results.AsTranslatingAsyncEnumerable(e => e.ToImmutable());
    }

    public async Task<IAsyncEnumerable<TroupeContract>> GetTroupesByIds(IEnumerable<string> troupeIds)
    {
        var troupeObjectIds = troupeIds.Select(id => new ObjectId(id)).ToArray();
        var results = await _troupes.FindAsync(t => troupeObjectIds.Contains(t.Id));
        return results.AsTranslatingAsyncEnumerable(e => e.ToImmutable());
    }

    public async Task<IAsyncEnumerable<VenueContract>> GetAllVenuesWithoutHalls()
    {
        var allVenues = (await _venues.FindAsync(v => true)).AsAsyncEnumerable();
        return IterateOverResults();

        async IAsyncEnumerable<VenueContract> IterateOverResults()
        {
            await foreach (var venue in allVenues)
            {
                yield return venue.ToImmutable() with {
                    Halls = ImmutableList<HallContract>.Empty
                };
            }
        }
    }

    public async Task<VenueContract?> TryGetVenueById(string venueId)
    {
        var results = await _venues.FindAsync(v => v.Id == new ObjectId(venueId));
        return (await results.FirstOrDefaultAsync())?.ToImmutable();
    }

    public async Task<IAsyncEnumerable<HallSeatingMapContract>> GetAllHallSeatingMapsWithoutAreas()
    {
        var allSeatingMaps = (await _hallSeatingMaps.FindAsync(m => true)).AsAsyncEnumerable();
        return IterateOverResults();

        async IAsyncEnumerable<HallSeatingMapContract> IterateOverResults()
        {
            await foreach (var map in allSeatingMaps)
            {
                yield return map.ToImmutable() with {
                    Areas = ImmutableList<AreaSeatingMapContract>.Empty
                };
            }
        }
    }

    public async Task<HallSeatingMapContract?> TryGetHallSeatingMapById(string hallSeatingMapId)
    {
        var results = 
            await _hallSeatingMaps.FindAsync(m => m.Id == new ObjectId(hallSeatingMapId));
        
        return (await results.FirstOrDefaultAsync())?.ToImmutable();
    }

    public void InsertInitialData()
    {
        _events.InsertMany(MockDatabase.Events.All.Select(e => new EventContractForDb(e)));
        _shows.InsertMany(MockDatabase.Shows.All.Select(s => new ShowContractForDb(s)));
        _showTypes.InsertMany(MockDatabase.ShowTypes.All.Select(st => new ShowTypeContractForDb(st)));
        _genres.InsertMany(MockDatabase.Genres.All.Select(g => new GenreContractForDb(g)));
        _venues.InsertMany(MockDatabase.Venues.All.Select(v => new VenueContractForDb(v)));
        _troupes.InsertMany(MockDatabase.Troupes.All.Select(t => new TroupeContractForDb(t)));
        _hallSeatingMaps.InsertMany(MockDatabase.HallSeatingMaps.All.Select(m => new HallSeatingMapContractForDb(m)));
    }

    public bool ShouldInsertInitialData()
    {
        return _events.CountDocuments(e => true) == 0;
    }

    public class EventContractForDb
    {
        public EventContractForDb()
        {
        }

        public EventContractForDb(EventContract source)
        {
            Id = new ObjectId(source.Id); 
            VenueId = new ObjectId(source.VenueId); 
            HallId = new ObjectId(source.HallId); 
            HallSeatingMapId = new ObjectId(source.HallSeatingMapId); 
            ShowTypeId = new ObjectId(source.ShowTypeId); 
            ShowId = new ObjectId(source.ShowId); 
            SaleStartUtc = source.SaleStartUtc; 
            EventStartUtc = source.EventStartUtc; 
            PriceList = new EventPriceListContractForDb(source.PriceList); 
            IsOpenForSale = source.IsOpenForSale; 
            DurationMinutes = source.DurationMinutes; 
            Title = source. Title; 
            Description = source. Description; 
            PosterImageUrl = source. PosterImageUrl;
            TroupeIds = source.TroupeIds?.Select(id => new ObjectId(id)).ToList();
        }

        public EventContract ToImmutable()
        {
            return new EventContract(
                Id.ToString(),
                VenueId.ToString(),
                HallId.ToString(),
                HallSeatingMapId.ToString(),
                ShowTypeId.ToString(),
                ShowId.ToString(),
                SaleStartUtc,
                EventStartUtc,
                PriceList.ToImmutable(),
                IsOpenForSale,
                DurationMinutes,
                Title,
                Description,
                PosterImageUrl,
                TroupeIds?.Select(id => id.ToString()).ToImmutableList()
            );
        }
        
        public ObjectId Id { get; set; } 
        public ObjectId VenueId { get; set; } 
        public ObjectId HallId { get; set; } 
        public ObjectId HallSeatingMapId { get; set; } 
        public ObjectId ShowTypeId { get; set; } 
        public ObjectId ShowId { get; set; } 
        public DateTime SaleStartUtc { get; set; } 
        public DateTime EventStartUtc { get; set; }
        public EventPriceListContractForDb PriceList { get; set; } = new EventPriceListContractForDb(); 
        public bool IsOpenForSale { get; set; } 
        public int DurationMinutes { get; set; } 
        public string? Title { get; set; } 
        public string? Description { get; set; } 
        public string? PosterImageUrl { get; set; } 
        public List<ObjectId>? TroupeIds { get; set; } 
    }

    public class EventPriceListContractForDb
    {
        public EventPriceListContractForDb()
        {
        }

        public EventPriceListContractForDb(EventPriceListContract source)
        {
            PriceByLevelId = new Dictionary<ObjectId, decimal>(source.PriceByLevelId
                .Select(kvp => new KeyValuePair<ObjectId, decimal>(
                    new ObjectId(kvp.Key),
                    kvp.Value)));
        }

        public EventPriceListContract ToImmutable()
        {
            return new EventPriceListContract(
                PriceByLevelId: ImmutableDictionary<string, decimal>.Empty.AddRange(
                    PriceByLevelId
                        .Select(kvp => new KeyValuePair<string, decimal>(
                            kvp.Key.ToString(),
                            kvp.Value))
                )
            ); 
        }
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ObjectId, decimal> PriceByLevelId { get; set; }
    }

    public class ShowContractForDb
    {
        public ShowContractForDb()
        {
        }

        public ShowContractForDb(ShowContract source)
        {
            Id = new ObjectId(source.Id);
            ShowTypeId = new ObjectId(source.ShowTypeId);
            GenreId = new ObjectId(source.GenreId);
            TroupeIds = source.TroupeIds?.Select(id => new ObjectId(id)).ToList();
            Title = source.Title;
            Description = source.Description;
            PosterImageUrl = source.PosterImageUrl;
        }

        public ShowContract ToImmutable()
        {
            return new ShowContract(
                Id.ToString(),
                ShowTypeId.ToString(),
                GenreId.ToString(),
                TroupeIds: TroupeIds?.Select(id => id.ToString()).ToImmutableList(),
                Title,
                Description,
                PosterImageUrl
            );
        }
        
        public ObjectId Id { get; set; } 
        public ObjectId ShowTypeId { get; set; }
        public ObjectId GenreId { get; set; }
        public List<ObjectId>? TroupeIds { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterImageUrl { get; set; }
    }

    public class ShowTypeContractForDb
    {
        public ShowTypeContractForDb()
        {
        }

        public ShowTypeContractForDb(ShowTypeContract source)
        {
            Id = new ObjectId(source.Id); 
            Name = source.Name;
            PosterImageUrl = source.PosterImageUrl;
        }

        public ShowTypeContract ToImmutable()
        {
            return new ShowTypeContract(
                Id.ToString(),
                Name,
                PosterImageUrl
            );
        }
        
        public ObjectId Id { get; set; } 
        public string Name { get; set; }
        public string PosterImageUrl { get; set; }
    }
    
    public class GenreContractForDb
    {
        public GenreContractForDb()
        {
        }

        public GenreContractForDb(GenreContract source)
        {
            Id = new ObjectId(source.Id); 
            Name = source.Name;
            PosterImageUrl = source.PosterImageUrl;
        }

        public GenreContract ToImmutable()
        {
            return new GenreContract(
                Id.ToString(),
                Name,
                PosterImageUrl
            );
        }
        
        public ObjectId Id { get; set; } 
        public string Name { get; set; }
        public string PosterImageUrl { get; set; }
    }

    public class TroupeContractForDb
    {
        public TroupeContractForDb()
        {
        }

        public TroupeContractForDb(TroupeContract source)
        {
            Id = new ObjectId(source.Id);
            GenreId  = new ObjectId(source.GenreId);
            Name = source.Name;
            PosterImageUrl = source.PosterImageUrl;
        }

        public TroupeContract ToImmutable()
        {
            return new TroupeContract(
                Id.ToString(),
                GenreId.ToString(),
                Name,
                PosterImageUrl
            );
        }
        
        public ObjectId Id { get; set; } 
        public ObjectId GenreId { get; set; } 
        public string Name { get; set; }
        public string PosterImageUrl { get; set; }
    }

    public class VenueContractForDb
    {
        public VenueContractForDb()
        {
        }

        public VenueContractForDb(VenueContract source)
        {
            Id = new ObjectId(source.Id);    
            Name = source.Name;    
            Address = source.Address; 
            Location = source.Location; 
            TimeZone = source.TimeZone; 
            Description = source.Description; 
            WebSiteUrl = source.WebSiteUrl; 
            PhotoImageUrl = source.PhotoImageUrl; 
            DefaultCapacity = source.DefaultCapacity; 
            Halls = source.Halls.Select(h => new HallContractForDb(h)).ToList(); 
        }

        public VenueContract ToImmutable()
        {
            return new VenueContract(
                Id.ToString(),
                Name,
                Address,
                Location,
                TimeZone,
                Description,
                WebSiteUrl,
                PhotoImageUrl,
                DefaultCapacity,
                Halls.Select(h => h.ToImmutable()).ToImmutableList()
            );
        }
        
        public ObjectId Id { get; set; }   
        public string Name { get; set; }   
        public string Address { get; set; }
        public GeoPointContract Location { get; set; }
        public TimeZoneContract TimeZone { get; set; }
        public string Description { get; set; }
        public string WebSiteUrl { get; set; }
        public string PhotoImageUrl { get; set; }
        public int DefaultCapacity { get; set; }
        public List<HallContractForDb> Halls { get; set; }
    }
    
    public class HallContractForDb
    {
        public HallContractForDb()
        {
        }

        public HallContractForDb(HallContract source)
        {
            Id = new ObjectId(source.Id); 
            Name = source.Name;
            SeatingPlanImageUrl = source.SeatingPlanImageUrl;
            Areas = source.Areas.Select(a => new HallAreaContractForDb(a)).ToList();
            DefaultSeatingMapId  = new ObjectId(source.DefaultSeatingMapId);
        }

        public HallContract ToImmutable()
        {
            return new HallContract(
                Id.ToString(),
                Name,
                SeatingPlanImageUrl,
                Areas.Select(a => a.ToImmutable()).ToImmutableList(),
                DefaultSeatingMapId.ToString()
            );
        }
        
        public ObjectId Id { get; set; } 
        public string Name { get; set; }
        public string SeatingPlanImageUrl { get; set; }
        public List<HallAreaContractForDb> Areas { get; set; }
        public ObjectId DefaultSeatingMapId { get; set; }
    }
    
    public class HallAreaContractForDb
    {
        public HallAreaContractForDb()
        {
        }

        public HallAreaContractForDb(HallAreaContract source)
        {
            Id = source.Id; 
            Name = source.Name;
            SeatingPlanImageUrl = source.SeatingPlanImageUrl;
        }

        public HallAreaContract ToImmutable()
        {
            return new HallAreaContract(
                Id,
                Name,
                SeatingPlanImageUrl
            );
        }
        
        public string Id { get; set; } 
        public string Name { get; set; }
        public string SeatingPlanImageUrl { get; set; }
    }

    public class PriceLevelContractForDb
    {
        public PriceLevelContractForDb()
        {
        }

        public PriceLevelContractForDb(PriceLevelContract source)
        {
            Id = new ObjectId(source.Id); 
            Name = source.Name;
            ColorHexRgb = source.ColorHexRgb;
        }

        public PriceLevelContract ToImmutable()
        {
            return new PriceLevelContract(
                Id.ToString(),
                Name,
                ColorHexRgb
            );
        }
        
        public ObjectId Id { get; set; } 
        public string Name { get; set; }
        public string ColorHexRgb { get; set; }
    }

    public class HallSeatingMapContractForDb
    {
        public HallSeatingMapContractForDb()
        {
        }

        public HallSeatingMapContractForDb(HallSeatingMapContract source)
        {
            Id = new ObjectId(source.Id);
            HallId = source.HallId;
            Name = source.Name;
            PlanImageUrl = source.PlanImageUrl;
            Capacity = source.Capacity;
            PriceLevels = source.PriceLevels.Select(pl => new PriceLevelContractForDb(pl)).ToList();
            Areas = source.Areas.Select(a => new AreaSeatingMapContractForDb(a)).ToList();
        }

        public HallSeatingMapContract ToImmutable()
        {
            return new HallSeatingMapContract(
                Id.ToString(),
                HallId,
                Name,
                PlanImageUrl,
                Capacity,
                PriceLevels.Select(pl => pl.ToImmutable()).ToImmutableList(),
                Areas.Select(a => a.ToImmutable()).ToImmutableList()
            );
        }
        
        public ObjectId Id { get; set; }
        public string HallId { get; set; }
        public string Name { get; set; }
        public string PlanImageUrl { get; set; }
        public int Capacity { get; set; }
        public List<PriceLevelContractForDb> PriceLevels { get; set; }
        public List<AreaSeatingMapContractForDb> Areas { get; set; }
    }

    public class AreaSeatingMapContractForDb
    {
        public AreaSeatingMapContractForDb()
        {
        }

        public AreaSeatingMapContractForDb(AreaSeatingMapContract source)
        {
            SeatingMapId = new ObjectId(source.SeatingMapId); 
            HallAreaId = new ObjectId(source.HallAreaId); 
            HallAreaName = source.HallAreaName; 
            PlanImageUrl = source.PlanImageUrl; 
            Capacity = source.Capacity; 
            Rows = source.Rows.Select(r => new SeatingMapRowContractForDb(r)).ToList(); 
        }

        public AreaSeatingMapContract ToImmutable()
        {
            return new AreaSeatingMapContract(
                SeatingMapId.ToString(),
                HallAreaId.ToString(),
                HallAreaName,
                PlanImageUrl,
                Capacity,
                Rows.Select(r => r.ToImmutable()).ToImmutableList()
            );
        }

        public ObjectId SeatingMapId { get; set; }
        public ObjectId HallAreaId { get; set; }
        public string HallAreaName { get; set; }
        public string PlanImageUrl { get; set; }
        public int Capacity { get; set; }
        public List<SeatingMapRowContractForDb> Rows { get; set; }
    }

    public class SeatingMapRowContractForDb
    {
        public SeatingMapRowContractForDb()
        {
        }

        public SeatingMapRowContractForDb(SeatingMapRowContract source)
        {
            Id = new ObjectId(source.Id);
            Name = source.Name;
            Seats = source.Seats.ToList();
        }

        public SeatingMapRowContract ToImmutable()
        {
            return new SeatingMapRowContract(
                Id.ToString(),
                Name,
                Seats.ToImmutableList()
            );
        }
        
        public ObjectId Id { get; set; }   
        public string Name { get; set; }
        public List<SeatingMapSeatContract> Seats { get; set; }
    }
}



// public static class ImmutableExtensions
// {
//     public static ImmutableDictionary<string, TValue> ToImmutableStringIdDictionary<TValue>(this Dictionary<ObjectId, TValue>)
// }