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

    public DiagnosticsController(IActorEngine actorEngine)
    {
        _actorEngine = actorEngine;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ClusterDiagnosticInfo> Get()
    {
        return ApiResult.Success(200, _actorEngine.GetClusterDiagnostics());
    }
}