#pragma warning disable CS1998
#pragma warning disable CS8618

using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Contracts;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.CheckoutService.Controllers;

[ApiController]
[Route("workflow")]
public class CheckoutWorkflowController : ControllerBase
{
    private readonly ICheckoutEntityRepository _entityRepo;

    public CheckoutWorkflowController(ICheckoutEntityRepository entityRepo)
    {
        _entityRepo = entityRepo;
    }

    [HttpPost("begin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<string>>> BeginWorkflow(
        [FromBody] BeginWorkflowRequest request)
    {
        Console.WriteLine($"CheckoutWorkflowController.BeginWorkflow number[{request.OrderNumber}] state[{request.StateName}] token[{request.TaskToken}]");

        try
        {
            var record = new WorkflowStateRecord(request.OrderNumber, request.StateName, request.TaskToken);
            await _entityRepo.InsertWorkflowStateRecord(record);
            return ApiResult.Success(200, "OK");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ApiResult.Error(400, "AlreadyExists");
        }
    }
}
