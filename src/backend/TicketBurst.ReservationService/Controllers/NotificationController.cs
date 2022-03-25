﻿using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Jobs;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Controllers;

[ApiController]
[Route("notify")]
public class NotificationController : ControllerBase
{
    private readonly EventWarmupJob _warmupJob;

    public NotificationController(EventWarmupJob warmupJob)
    {
        _warmupJob = warmupJob;
    }

    [HttpPost("event-sale")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public ActionResult<ReplyContract<string>> HandleEventSaleNotification(
        [FromBody] EventSaleNotificationContract notification)
    {
        var isValid = ValidateNotification(out var validationResult);
        if (isValid)
        {
            _warmupJob.Enqueue(notification.EventId, notification.HallAreaIds);
        }
        
        var reply = new ReplyContract<string>(
            validationResult, 
            ServiceProcessMetadata.GetCombinedInfo());
        
        return new JsonResult(reply) {
            StatusCode = isValid ? 200 : 400
        };

        bool ValidateNotification(out string resultCode)
        {
            var timeSincePublish = DateTime.UtcNow.Subtract(notification.PublishedAtUtc);
            var timeToSaleStart = new TimeSpan(Math.Abs(DateTime.UtcNow.Subtract(notification.SaleStartUtc).Ticks));
            if (timeSincePublish < TimeSpan.Zero || timeToSaleStart > TimeSpan.FromHours(1))
            {
                resultCode = "BadTiming";
                return false;
            }

            if (string.IsNullOrWhiteSpace(notification.Id) || 
                string.IsNullOrWhiteSpace(notification.EventId) ||
                notification.HallAreaIds == null ||
                notification.HallAreaIds.Count == 0 || 
                notification.HallAreaIds.Any(string.IsNullOrWhiteSpace))
            {
                resultCode = "RequiredFieldsMissing";
                return false;
            }
            
            resultCode = "OK";
            return true;
        }
    }
}
