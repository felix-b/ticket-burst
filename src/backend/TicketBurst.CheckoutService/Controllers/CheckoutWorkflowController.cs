#pragma warning disable CS1998

using System.Collections.Immutable;
using System.Net.Mail;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Integrations;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;
#pragma warning disable CS8618

namespace TicketBurst.CheckoutService.Controllers;

[ApiController]
[Route("checkout")]
public class CheckoutWorkflowController : ControllerBase
{
    [HttpPost("begin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task BeginWorkflow(
        [FromBody] BeginWorkflowRequest request)
    {
        Console.WriteLine("--------BeginWorkflow---------");
        Console.WriteLine($"> order number [{request.OrderNumber}]");
        Console.WriteLine($"> task token [{request.TaskToken}]");
    }

    public class BeginWorkflowRequest
    {
        public uint OrderNumber { get; set; }
        public string TaskToken { get; set; }
    }
}