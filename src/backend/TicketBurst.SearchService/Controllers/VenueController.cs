using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("venue")]
public class VenueController : ControllerBase
{
    private readonly ISearchEntityRepository _entityRepo;

    public VenueController(
        ISearchEntityRepository entityRepo,
        ILogger<VenueController> logger)
    {
        _entityRepo = entityRepo;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<VenueContract>>>> Get()
    {
        var data  = (await _entityRepo.GetAllVenuesWithoutHalls()).ToListSync();
        
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
    public async Task<ActionResult<ReplyContract<IEnumerable<HallContract>>>> GetHalls(string id)
    {
        var venue = await _entityRepo.TryGetVenueById(id);
        var data = venue?.Halls;
        
        var reply = new ReplyContract<IEnumerable<HallContract>>(
            data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = data != null ? 200 : 404
        };
    }
}
