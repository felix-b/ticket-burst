using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("venue")]
public class VenueController : ControllerBase
{
    public VenueController(ILogger<VenueController> logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<ReplyContract<IEnumerable<VenueContract>>> Get()
    {
        var data = MockDatabase.Venues.All
            .Select(x => x with {
                Halls = ImmutableList<HallContract>.Empty
            });
        
        var reply = new ReplyContract<IEnumerable<VenueContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = 200
        };
    }

    [HttpGet("{id}/halls")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ReplyContract<IEnumerable<HallContract>>> GetHalls(string id)
    {
        var data =  MockDatabase.Venues.All
            .FirstOrDefault(v => v.Id == id)
            ?.Halls;
        
        var reply = new ReplyContract<IEnumerable<HallContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }
}
