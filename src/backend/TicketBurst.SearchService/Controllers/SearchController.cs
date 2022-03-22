using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

public class EventSearchQuery
{
    public DateTime? FromDate { get; set; }    
    public DateTime? ToDate { get; set; }    
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
    public ActionResult<ReplyContract<IEnumerable<EventSearchResultContract>>> Get([FromQuery] EventSearchQuery query)
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
}
