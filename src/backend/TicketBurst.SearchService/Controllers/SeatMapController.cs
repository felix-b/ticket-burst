using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("seatmap")]
public class SeatMapController : ControllerBase
{
    public SeatMapController(ILogger<SeatMapController> logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<HallSeatingMapContract>>> Get()
    {
        var data =  MockDatabase.HallSeatingMaps.All
            .Select(map => map with {
                Areas = ImmutableList<AreaSeatingMapContract>.Empty
            });
        
        var reply = new ReplyContract<IEnumerable<HallSeatingMapContract>>(
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
    public ActionResult<ReplyContract<HallSeatingMapContract?>> GetMap(string id)
    {
        var data =  MockDatabase.HallSeatingMaps.All
            .FirstOrDefault(m => m.Id == id);
        
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
    public ActionResult<ReplyContract<IEnumerable<AreaSeatingMapContract>>> GetMapAreaList(string seatingMapId)
    {
        var data = MockDatabase.HallSeatingMaps.All
            .FirstOrDefault(m => m.Id == seatingMapId)
            ?.Areas.Select(a => a with {
                Rows = ImmutableList<SeatingMapRowContract>.Empty
            });
        
        var reply = new ReplyContract<IEnumerable<AreaSeatingMapContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }

    [HttpGet("{seatingMapId}/area/{hallAreaId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ReplyContract<AreaSeatingMapContract>> GetMapArea(string seatingMapId, string hallAreaId)
    {
        var data =  MockDatabase.HallSeatingMaps.All
            .FirstOrDefault(m => m.Id == seatingMapId)
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
