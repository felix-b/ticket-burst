using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("capacity")]
public class CapacityController : ControllerBase
{
    public CapacityController(ILogger<CapacityController> logger)
    {
    }

    [HttpPost("update")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public ActionResult<ReplyContract<string>> UpdateAreaCapacity(
        [FromBody] EventAreaUpdateNotificationContract notification)
    {
        var isValid = ValidateNotification(out var validationResult);
        if (isValid)
        {
            MockDatabase.EventSeatingStatusCache.Update(notification);
        }
        
        var reply = new ReplyContract<string>(
            validationResult, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = isValid ? 200 : 400
        };

        bool ValidateNotification(out string resultCode)
        {
            var timeSincePublish = DateTime.UtcNow.Subtract(notification.PublishedAtUtc);
            
            if (string.IsNullOrWhiteSpace(notification.Id) || string.IsNullOrWhiteSpace(notification.EventId) || string.IsNullOrWhiteSpace(notification.HallAreaId))
            {
                resultCode = "RequiredFieldsMissing";
                return false;
            }
            
            if (notification.SequenceNo <= 0)
            {
                resultCode = "BadSequenceNo";
                return false;
            }
            
            if (timeSincePublish < TimeSpan.Zero || timeSincePublish > TimeSpan.FromMinutes(1))
            {
                resultCode = "BadTiming";
                return false;
            }

            if (notification.TotalCapacity <= 0 || 
                notification.TotalCapacity > 1000000 || 
                notification.AvailableCapacity < 0 || 
                notification.AvailableCapacity > notification.TotalCapacity)
            {
                resultCode = "BadCounterValues";
                return false;
            }

            var @event = MockDatabase.Events.All.FirstOrDefault(e => e.Id == notification.EventId);
            if (@event == null)
            {
                resultCode = "EventNotFound";
                return false;
            }

            var area = MockDatabase.Venues.All
                .FirstOrDefault(v => v.Id == @event.VenueId)
                ?.Halls[0]
                .Areas.FirstOrDefault(a => a.Id == notification.HallAreaId);
                
            if (area == null)
            {
                resultCode = "AreaNotFound";
                return false;
            }

            resultCode = "OK";
            return true;
        }
    }
}
