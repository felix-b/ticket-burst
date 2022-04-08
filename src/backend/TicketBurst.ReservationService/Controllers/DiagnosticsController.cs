using Microsoft.AspNetCore.Mvc;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ReservationService.Integrations.ProtoActor;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Controllers;

[ApiController]
[Route("diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly IActorEngine _actorEngine;
    private readonly IClusterInfoProvider _clusterInfo;

    public DiagnosticsController(IActorEngine actorEngine, IClusterInfoProvider clusterInfo)
    {
        _actorEngine = actorEngine;
        _clusterInfo = clusterInfo;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ClusterInfo> Get()
    {
        return ApiResult.Success(200, _clusterInfo.Current);
    }
}
