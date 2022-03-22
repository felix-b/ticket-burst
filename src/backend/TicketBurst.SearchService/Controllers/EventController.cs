using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("event")]
public class EventController : ControllerBase
{
    public EventController(ILogger<EventController> logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<EventContract>>> Get()
    {
        var data = MockDatabase.Events.All;
        
        var reply = new ReplyContract<IEnumerable<EventContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("genre")]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<GenreContract>>> GetGenres()
    {
        var data = MockDatabase.Genres.All;
        
        var reply = new ReplyContract<IEnumerable<GenreContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("showtype")]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<ShowTypeContract>>> GetShowTypes()
    {
        var data = MockDatabase.ShowTypes.All;
        
        var reply = new ReplyContract<IEnumerable<ShowTypeContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("show")]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<ShowContract>>> GetShows()
    {
        var data = MockDatabase.Shows.All;
        
        var reply = new ReplyContract<IEnumerable<ShowContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("troupe")]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<TroupeContract>>> GetTroupes()
    {
        var data = MockDatabase.Troupes.All;
        
        var reply = new ReplyContract<IEnumerable<TroupeContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ReplyContract<EventContract>> GetEvent(string id)
    {
        var data = MockDatabase.Events.All
            .FirstOrDefault(e => e.Id == id);
        
        var reply = new ReplyContract<EventContract>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }

    [HttpGet("{id}/areas")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ReplyContract<IEnumerable<HallAreaContract>>> GetEventAreas(string id)
    {
        var data = FetchData();
        
        var reply = new ReplyContract<IEnumerable<HallAreaContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
        
        IEnumerable<HallAreaContract>? FetchData()
        {
            var @event = MockDatabase.Events.All.FirstOrDefault(e => e.Id == id);
            if (@event == null)
            {
                return null;
            }
            
            var venue = MockDatabase.Venues.All.FirstOrDefault(v => v.Id == @event.VenueId);
            return venue?.Halls[0].Areas;
        }
    }
    
    [HttpGet("{id}/area-seatmap/{hallAreaId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ReplyContract<AreaSeatingMapContract>> GetEventAreaSeatingMap(string id, string hallAreaId)
    {
        var data = FetchData();
        
        var reply = new ReplyContract<AreaSeatingMapContract>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
        
        AreaSeatingMapContract? FetchData()
        {
            var @event = MockDatabase.Events.All.FirstOrDefault(e => e.Id == id);
            if (@event == null)
            {
                return null;
            }
            
            var venue = MockDatabase.Venues.All.FirstOrDefault(v => v.Id == @event.VenueId);
            if (venue == null)
            {
                return null;
            }

            var seatingMap = MockDatabase.HallSeatingMaps.All.FirstOrDefault(m => m.Id == @event.HallSeatingMapId);
            if (seatingMap == null)
            {
                return null;
            }

            return seatingMap.Areas.FirstOrDefault(a => a.HallAreaId == hallAreaId);
        }
    }
}
