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
    private readonly SimpleStatefulClusterMember _clusterMember;

    public DiagnosticsController(IActorEngine actorEngine, SimpleStatefulClusterMember clusterMember)
    {
        _actorEngine = actorEngine;
        _clusterMember = clusterMember;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<SimpleStatefulClusterState> Get()
    {
        return ApiResult.Success(200, _clusterMember.CurrentState);
    }
}
