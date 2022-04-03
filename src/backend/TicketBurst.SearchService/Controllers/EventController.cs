using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("event")]
public class EventController : ControllerBase
{
    private readonly ISearchEntityRepository _entityRepo;

    public EventController(
        ISearchEntityRepository entityRepo,
        ILogger<EventController> logger)
    {
        _entityRepo = entityRepo;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<EventContract>>>> Get()
    {
        var data = await _entityRepo.GetAllEvents();
        return ApiResult.Success(200, data.ToListSync());
    }

    [HttpGet("genre")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<GenreContract>>>> GetGenres()
    {
        var data = await _entityRepo.GetAllGenres();
        return ApiResult.Success(200, data.ToListSync());
    }

    [HttpGet("showtype")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<ShowTypeContract>>>> GetShowTypes()
    {
        var data = await _entityRepo.GetAllShowTypes();
        return ApiResult.Success(200, data.ToListSync());
    }

    [HttpGet("show")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<ShowContract>>>> GetShows()
    {
        var data = await _entityRepo.GetAllShows();
        return ApiResult.Success(200, data.ToListSync());
    }

    [HttpGet("troupe")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<TroupeContract>>>> GetTroupes()
    {
        var data = await _entityRepo.GetAllTroupes();
        return ApiResult.Success(200, data.ToListSync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<EventContract>>> GetEvent(string id)
    {
        var data = await _entityRepo.TryGetEventById(id);
        return data != null
            ? ApiResult.Success(200, data)
            : ApiResult.Error(404);
    }

    [HttpGet("{id}/areas")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<IEnumerable<HallAreaContract>>>> GetEventAreas(string id)
    {
        var data = await FetchData();
        return data != null
            ? ApiResult.Success(200, data)
            : ApiResult.Error(404);
        
        async Task<IEnumerable<HallAreaContract>?> FetchData()
        {
            var @event =  await _entityRepo.TryGetEventById(id);;
            if (@event == null)
            {
                return null;
            }
            
            var venue = await _entityRepo.TryGetVenueById(@event.VenueId);
            return venue?.Halls[0].Areas;
        }
    }
    
    [HttpGet("{id}/area-seatmap/{hallAreaId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<AreaSeatingMapContract>>> GetEventAreaSeatingMap(string id, string hallAreaId)
    {
        var data = await FetchData();
        return data != null
            ? ApiResult.Success(200, data)
            : ApiResult.Error(404);
        
        async Task<AreaSeatingMapContract?> FetchData()
        {
            var @event = await _entityRepo.TryGetEventById(id);
            if (@event == null)
            {
                return null;
            }
            
            var seatingMap = await _entityRepo.TryGetHallSeatingMapById(@event.HallSeatingMapId);
            if (seatingMap == null)
            {
                return null;
            }

            return seatingMap.Areas.FirstOrDefault(a => a.HallAreaId == hallAreaId);
        }
    }
}
