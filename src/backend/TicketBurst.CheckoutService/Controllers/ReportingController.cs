using TicketBurst.CheckoutService.Integrations;

namespace TicketBurst.CheckoutService.Controllers;

using Microsoft.AspNetCore.Mvc;
using TicketBurst.CheckoutService.Contracts;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

[ApiController]
[Route("reporting")]
public class ReportingController : ControllerBase
{
    private readonly ICheckoutEntityRepository _entityRepo;

    public ReportingController(ICheckoutEntityRepository entityRepo)
    {
        _entityRepo = entityRepo;
    }

    [HttpPost("add-order")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<string>>> AddOrder(
        [FromBody] CheckoutWorkflowStateContract.OrderPart order)
    {
        Console.WriteLine($"ReportingController.AddOrder [{order.OrderNumber}]");

        try
        {
            var record = new AggregatedSalesRecord {
                OrderDate = order.OrderDate,   
                EventDate = order.EventDate,   
                VenueId = order.VenueId,   
                EventId = order.EventId,   
                AreaId = order.AreaId,   
                PriceLevelId = order.PriceLevelId,
                TicketCount = order.TicketCount,
            };
            await _entityRepo.UpsertAggregatedSalesRecord(record);
            return ApiResult.Success(200, "OK");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ApiResult.Error(400, "AlreadyExists");
        }
    }
    
    [HttpGet("/recent-sales-aggregated/{count}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ReplyContract<IEnumerable<AggregatedSalesRecord>>>> GetRecentSales(int count)
    {
        var data = await _entityRepo.GetRecentAggregatedSales(count);
        return ApiResult.Success(200, data);
    }
}
