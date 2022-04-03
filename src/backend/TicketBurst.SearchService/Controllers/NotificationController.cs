using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.SearchService.Logic;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("notify")]
public class NotificationController : ControllerBase
{
    private readonly ISearchEntityRepository _entityRepo;
    private readonly EventSeatingStatusCache _eventSeatingCache;

    public NotificationController(
        ISearchEntityRepository entityRepo,
        EventSeatingStatusCache eventSeatingCache,
        ILogger<NotificationController> logger)
    {
        _entityRepo = entityRepo;
        _eventSeatingCache = eventSeatingCache;
    }

    [HttpPost("event-area-update")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ReplyContract<string>>> HandleEventAreaUpdateNotification(
        [FromBody] EventAreaUpdateNotificationContract notification)
    {
        var validationErrorCode = await ValidateNotification();
        var isValid = string.IsNullOrEmpty(validationErrorCode); 
        
        if (isValid)
        {
            _eventSeatingCache.Update(notification);
        }

        return isValid
            ? ApiResult.Success(200, "OK")
            : ApiResult.Error(400, validationErrorCode);

        async Task<string> ValidateNotification()
        {
            var timeSincePublish = DateTime.UtcNow.Subtract(notification.PublishedAtUtc);
            
            if (string.IsNullOrWhiteSpace(notification.Id) || string.IsNullOrWhiteSpace(notification.EventId) || string.IsNullOrWhiteSpace(notification.HallAreaId))
            {
                return "RequiredFieldsMissing";
            }
            
            if (notification.SequenceNo <= 0)
            {
                return "BadSequenceNo";
            }
            
            if (timeSincePublish < TimeSpan.Zero || timeSincePublish > TimeSpan.FromMinutes(1))
            {
                return "BadTiming";
            }

            if (notification.TotalCapacity <= 0 || 
                notification.TotalCapacity > 1000000 || 
                notification.AvailableCapacity < 0 || 
                notification.AvailableCapacity > notification.TotalCapacity)
            {
                return "BadCounterValues";
            }

            var @event = await _entityRepo.TryGetEventById(notification.EventId);
            if (@event == null)
            {
                return "EventNotFound";
            }

            var venue = await _entityRepo.TryGetVenueById(@event.VenueId);
            var area = venue?
                .Halls[0]
                .Areas.FirstOrDefault(a => a.Id == notification.HallAreaId);
                
            if (area == null)
            {
                return "AreaNotFound";
            }

            return string.Empty;
        }
    }
}
