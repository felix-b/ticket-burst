using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("seatmap")]
public class SeatMapController : ControllerBase
{
    private readonly ISearchEntityRepository _entityRepo;

    public SeatMapController(
        ISearchEntityRepository entityRepo,
        ILogger<SeatMapController> logger)
    {
        _entityRepo = entityRepo;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<HallSeatingMapContract>>>> Get()
    {
        var data = await _entityRepo.GetAllHallSeatingMapsWithoutAreas();
        
        var reply = new ReplyContract<IEnumerable<HallSeatingMapContract>>(
            data.ToListSync(), 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<HallSeatingMapContract?>>> GetMap(string id)
    {
        var data = await _entityRepo.TryGetHallSeatingMapById(id);
        
        var reply = new ReplyContract<HallSeatingMapContract?>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }

    [HttpGet("{seatingMapId}/areas")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<IEnumerable<AreaSeatingMapContract>>>> GetMapAreaList(string seatingMapId)
    {
        var data = await _entityRepo.TryGetHallSeatingMapWithoutSeats(seatingMapId);
        
        var reply = new ReplyContract<IEnumerable<AreaSeatingMapContract>>(
            data?.Areas, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }

    [HttpGet("{seatingMapId}/area/{hallAreaId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReplyContract<AreaSeatingMapContract>>> GetMapArea(string seatingMapId, string hallAreaId)
    {
        var hallSeatingMap = await _entityRepo.TryGetHallSeatingMapById(seatingMapId);
        var data =  hallSeatingMap
            ?.Areas.FirstOrDefault(a => a.HallAreaId == hallAreaId);
        
        var reply = new ReplyContract<AreaSeatingMapContract>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }
}
