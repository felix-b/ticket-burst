#pragma warning disable CS1998

using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.SearchService.Logic;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

public class EventSearchRequestContract
{
    public string? Id { get; set; }
    public DateTime? FromDate { get; set; }    
    public DateTime? ToDate { get; set; }
    public bool? Selling { get; set; }
    public int? Count { get; set; }
}

[ApiController]
[Route("search")]
public class SearchController : ControllerBase
{
    private readonly ISearchEntityRepository _entityRepo;
    private readonly EventSeatingStatusCache _seatingCache;

    public SearchController(
        ISearchEntityRepository entityRepo,
        EventSeatingStatusCache seatingCache,
        ILogger<SearchController> logger)
    {
        _entityRepo = entityRepo;
        _seatingCache = seatingCache;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<EventSearchResultContract>>>> Get(
        [FromQuery] EventSearchRequestContract request)
    {
        var query = BuildQuery();
        var maxResultCount = Math.Min(100, request.Count ?? 20); 
        var foundEvents = query
            .Take(maxResultCount)
            .ToArray(); //TODO: async?
            
        var results = new List<EventSearchResultContract>();

        foreach (var @event in foundEvents)
        {
            var result = await EnrichEventToSearchResult(@event);
            results.Add(result);
        }

        return ApiResult.Success(200, results);

        //
        // var reply = new ReplyContract<IEnumerable<EventSearchResultContract>>(
        //     results, 
        //     ServiceProcessMetadata.GetCombinedInfo()
        // );
        //
        // return new JsonResult(reply) {
        //     StatusCode = 200
        // };

        IQueryable<EventContract> BuildQuery()
        {
            var query = _entityRepo.CreateEventQuery();

            if (request.Selling == true)
            {
                query = query.OrderByDescending(e => e.IsOpenForSale);
            }

            if (!string.IsNullOrWhiteSpace(request.Id))
            {
                query = query.Where(e => e.Id == request.Id);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(e => e.EventStartUtc >= request.FromDate.Value.Date);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(e => e.EventStartUtc < request.ToDate.Value.Date.AddDays(1));
            }

            return query.OrderBy(e => e.EventStartUtc);
        }
    }
    
    [HttpGet("event/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<EventSearchFullDetailContract>>> GetEventFullDetail(string id)
    {
        var @event = await _entityRepo.TryGetEventById(id);
        if (@event == null)
        {
            return ApiResult.Error(404);
        }

        var enrichedEvent = await EnrichEventToSearchResult(@event);
        var hallStatusCache = _seatingCache.Retrieve(@event.Id);
        var hallInfo = GetFullDetailHallInfo(@event, hallStatusCache, out _);
        var fullDetail = new EventSearchFullDetailContract(
            Event: enrichedEvent,
            Hall: hallInfo,
            PriceList: @event.PriceList);

        return ApiResult.Success(200, fullDetail);
    }

    [HttpGet("event/{eventId}/area/{areaId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<EventSearchAreaSeatingContract>>> GetEventFullDetail(string eventId, string areaId)
    {
        var @event = await _entityRepo.TryGetEventById(eventId);
        if (@event == null)
        {
            return ApiResult.Error(404);
        }
        
        var hallStatusCache = _seatingCache.Retrieve(@event.Id);
        if (!hallStatusCache.SeatingByAreaId.ContainsKey(areaId))
        {
            return ApiResult.Error(404);
        }
        
        var enrichedEvent = await EnrichEventToSearchResult(@event);
        var hallInfo = GetFullDetailHallInfo(@event, hallStatusCache, out var hallSeatingMap);
        var header = new EventSearchFullDetailContract(
            Event: enrichedEvent,
            Hall: hallInfo,
            PriceList: @event.PriceList);

        var areaSeatingMapWithStatus = GetAreaSeatingMapWithStatus(hallSeatingMap, hallStatusCache, areaId);
        var reply = new EventSearchAreaSeatingContract(
            Header: header,
            PriceLevels: hallSeatingMap.PriceLevels,
            HallAreaId: areaId,
            HallAreaName: areaSeatingMapWithStatus.HallAreaName,
            AvailableCapacity: hallStatusCache.SeatingByAreaId[areaId].AvailableCapacity,
            SeatingMap: areaSeatingMapWithStatus);

        return ApiResult.Success(200, reply);
    }
    
    private async Task<EventSearchResultContract> EnrichEventToSearchResult(EventContract @event)
    {
        var venue = await _entityRepo.GetVenueByIdOrThrow(@event.VenueId);
        var hall = venue.Halls[0];
        var show = await _entityRepo.GetShowByIdOrThrow(@event.ShowId);
        var showType = await _entityRepo.GetShowTypeByIdOrThrow(@event.ShowTypeId);
        var genre = await _entityRepo.GetGenreByIdOrThrow(show.GenreId);
        var troupeIds = @event.TroupeIds ?? show.TroupeIds;
        var troupes = troupeIds != null
            ? (await _entityRepo.GetTroupesByIds(troupeIds)).ToListSync()
            : null;
        var numberOfSeatsLeft = (@event.IsOpenForSale
            ? _seatingCache.Retrieve(@event.Id).AvailableCapacity
            : DateTime.UtcNow < @event.EventStartUtc
                ? venue.DefaultCapacity
                : 0);
        
        var result = new EventSearchResultContract(
            EventId: @event.Id,
            HallId: hall.Id,
            HallName: hall.Name,
            VenueId: venue.Id,
            VenueName: venue.Name,
            VenueAddress: venue.Address,
            VenueLocation: venue.Location,
            VenueTimeZone: venue.TimeZone,
            ShowId: show.Id,
            ShowName: show.Title,
            ShowDescription: show.Description,
            ShowTypeId: showType.Id,
            ShowTypeName: showType.Name,
            GenreId: genre.Id,
            GenreName: genre.Name,
            EventTitle: $"{show.Title} {@event.Title ?? string.Empty}".Trim(),
            EventDescription: @event.Description,
            PosterImageUrl: @event.PosterImageUrl ?? show.PosterImageUrl,
            Troupes: troupes?.ToImmutableList(),
            SaleStartTime: @event.SaleStartUtc,
            EventStartTime: @event.EventStartUtc,
            DurationMinutes: @event.DurationMinutes,
            IsOpenForSale: @event.IsOpenForSale,
            MinPrice: @event.PriceList.PriceByLevelId.Values.Min(),
            MaxPrice: @event.PriceList.PriceByLevelId.Values.Max(),
            NumberOfSeatsLeft: numberOfSeatsLeft);

        return result;
    }

    private EventSearchFullDetailContract.HallInfo GetFullDetailHallInfo(
        EventContract @event, 
        EventSeatingCacheContract hallStatusCache,
        out HallSeatingMapContract hallSeatingMap)
    {
        hallSeatingMap = 
            _entityRepo.TryGetHallSeatingMapByIdSync(@event.HallSeatingMapId)
            ?? throw new InvalidDataException($"Hall seating map [{@event.HallSeatingMapId}] for event [{@event.Id}] not found");

        var areaInfos = hallSeatingMap.Areas.Select(area => {
            FindAreaPriceRange(@event.PriceList, area, out var minPrice, out var maxPrice);
            return new EventSearchFullDetailContract.AreaInfo(
                HallAreaId: area.HallAreaId,
                Name: area.HallAreaName,
                SeatingPlanImageUrl: area.PlanImageUrl,
                TotalCapacity: area.Capacity,
                AvailableCapacity: hallStatusCache.SeatingByAreaId[area.HallAreaId].AvailableCapacity,
                MinPrice: minPrice,
                MaxPrice: maxPrice
            );
        }).ToImmutableList();

        return new EventSearchFullDetailContract.HallInfo(
            SeatingPlanImageUrl: hallSeatingMap.PlanImageUrl,
            TotalCapacity: hallSeatingMap.Capacity,
            AvailableCapacity: hallStatusCache.AvailableCapacity,
            Areas: areaInfos,
            PriceLevels: hallSeatingMap.PriceLevels
        );
    }

    private void FindAreaPriceRange(
        EventPriceListContract priceList, 
        AreaSeatingMapContract map, 
        out decimal minPrice, 
        out decimal maxPrice)
    {
        var seatsTemp  = map.Rows
            .SelectMany(row => row.Seats)
            .ToArray();

        var badSeats = seatsTemp.Where(s => s.PriceLevelId == null).ToArray();
        if (badSeats.Length > 0)
        {
            Console.WriteLine("BAD SEATS FOUND!");
        }
            
        var pricesTemp = seatsTemp
            .Select(seat => seat.PriceLevelId)
            .ToArray();
        
        var prices = pricesTemp
            .Distinct()
            .Select(priceLevelId => priceList.PriceByLevelId[priceLevelId])
            .ToArray();
        
        minPrice = prices.Min();
        maxPrice = prices.Max();
    }

    private AreaSeatingMapContract GetAreaSeatingMapWithStatus(
        HallSeatingMapContract hallSeatingMap,
        EventSeatingCacheContract hallStatusCache, 
        string hallAreaId)
    {
        var areaStatusCache = hallStatusCache.SeatingByAreaId[hallAreaId];
        var areaSeatingMap = hallSeatingMap.Areas.First(area => area.HallAreaId == hallAreaId); //TODO: faster lookup?
        
        var areaSeatingMapWithStatus = areaSeatingMap with { 
            Rows = areaSeatingMap.Rows.Select(row => row with {
                Seats = row.Seats.Select(seat => seat with {
                    Status = areaStatusCache.AvailableSeatIds.Contains(seat.Id) 
                        ? SeatStatus.Available 
                        : SeatStatus.Sold
                }).ToImmutableList()
            }).ToImmutableList()
        };

        return areaSeatingMapWithStatus;
    }
}
