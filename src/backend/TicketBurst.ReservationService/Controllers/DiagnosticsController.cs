using Microsoft.AspNetCore.Mvc;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Controllers;

[ApiController]
[Route("diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly IClusterInfoProvider _infoProvider;
    private readonly SimpleStatefulClusterMember _clusterMember;
    private readonly IActorEngine _actorEngine;
    private readonly DevboxClusterOrchestrator? _devboxOrchestrator;

    public DiagnosticsController(
        IClusterInfoProvider infoProvider,  
        SimpleStatefulClusterMember clusterMember,
        IActorEngine actorEngine,
        IServiceProvider serviceProvider)
    {
        _infoProvider = infoProvider;
        _actorEngine = actorEngine;
        _clusterMember = clusterMember;
        _devboxOrchestrator = serviceProvider.GetService<DevboxClusterOrchestrator>();
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ClusterMemberDiagnostics> Get()
    {
        var result = new ClusterMemberDiagnostics(
            _infoProvider.TryGetInfo(),
            _clusterMember.CurrentState,
            _actorEngine.GetLocalActorIds()
        );
        
        return ApiResult.Success(200, result);
    }

    [HttpPost("devbox/scale")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public ActionResult<ClusterMemberDiagnostics> DevBoxScaleRequest([FromBody] int desiredCount)
    {
        if (_devboxOrchestrator == null)
        {
            return ApiResult.Error(400);
        }

        _devboxOrchestrator.RequestScale(desiredCount);
        return ApiResult.Success(200, "OK");
    }
    
    public record ClusterMemberDiagnostics(
        ClusterInfo? ClusterInfo,
        SimpleStatefulClusterState ClusterState,
        string[] ActorIds
    );
}
