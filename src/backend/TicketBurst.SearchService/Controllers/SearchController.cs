using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
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
    public SearchController(ILogger<SearchController> logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<EventSearchResultContract>>> Get(
        [FromQuery] EventSearchRequestContract request)
    {
        var query = BuildQuery();
        var results = query
            .Take(Math.Min(100, request.Count ?? 20))
            .ToArray()
            .Select(EnrichResult)
            .ToArray();
        
        var reply = new ReplyContract<IEnumerable<EventSearchResultContract>>(
            results, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };

        IQueryable<EventContract> BuildQuery()
        {
            var query = MockDatabase.Events.All.AsQueryable();

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

            if (request.Selling.HasValue)
            {
                query = query.Where(e => e.IsOpenForSale == request.Selling.Value);
            }
            
            return query;
        }

        EventSearchResultContract EnrichResult(EventContract result)
        {
            var venue = MockDatabase.Venues.All.First(v => v.Id == result.VenueId);
            var hall = venue.Halls[0];
            var show = MockDatabase.Shows.All.First(s => s.Id == result.ShowId);
            var showType = MockDatabase.ShowTypes.All.First(t => t.Id == result.ShowTypeId);
            var genre = MockDatabase.Genres.All.First(g => g.Id == show.GenreId);
            var isSelling = (
                result.SaleStartUtc < DateTime.UtcNow &&
                DateTime.UtcNow < result.EventStartUtc.AddMinutes(result.DurationMinutes));
            var numbrOfSeatsLeft = (isSelling
                ? MockDatabase.EventSeatingStatusCache.Retrieve(result.Id).AvailableCapacity
                : DateTime.UtcNow < result.EventStartUtc
                    ? venue.DefaultCapacity
                    : 0);
            
            return new EventSearchResultContract(
                EventId: result.Id,
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
                EventTitle: $"{show.Title} {result.Title ?? string.Empty}".Trim(),
                EventDescription: result.Description,
                PosterImageUrl: result.PosterImageUrl ?? show.PosterImageUrl,
                TroupeIds: result.TroupeIds ?? show.TroupeIds,
                SaleStartTime: result.SaleStartUtc,
                EventStartTime: result.EventStartUtc,
                DurationMinutes: result.DurationMinutes,
                CanBuyTickets: isSelling,
                NumberOfSeatsLeft: numbrOfSeatsLeft);
        }
    }
}
