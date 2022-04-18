using System.Diagnostics;
using TicketBurst.ServiceInfra;
using TicketBurst.SearchService.Jobs;

namespace TicketBurst.SearchService.Telemetry.Impl;

internal class EventSaleStatusJobTelemetry : IEventSaleStatusJobTelemetry 
{
    private static readonly string __s_EventSaleStatusUpdateJob = "EventSaleStatusUpdateJob";
    private static readonly string __s_EventSaleStatusUpdateJob_OpenEventSale = "EventSaleStatusUpdateJob.OpenEventSale";
    private static readonly string __s_EventSaleStatusUpdateJob_CloseEventSale = "EventSaleStatusUpdateJob.CloseEventSale";
    private static readonly string __s_eventId = "eventId";

    public ITelemetrySpan NotifyOpenEventSale(string eventId)
    {
        var activity = TelemetryServices.ActivitySource.StartActivity(nameof(NotifyOpenEventSale), ActivityKind.Producer);
        activity?.AddBaggage(nameof(eventId), eventId);
        return activity != null ? new ActivitySpan(activity) : NoopSpan.Instance;
    }

    public ITelemetrySpan NotifyCloseEventSale(string eventId)
    {
        var activity = TelemetryServices.ActivitySource.StartActivity(nameof(NotifyCloseEventSale), ActivityKind.Producer);
        activity?.AddBaggage(nameof(eventId), eventId);
        return activity != null ? new ActivitySpan(activity) : NoopSpan.Instance;
    }

    public void OpenEventSale(string eventId)
    {
        var tags = new KeyValuePair<string, object?>[] {
            new(__s_eventId, eventId)            
        };
        
        var e = new ActivityEvent(
            __s_EventSaleStatusUpdateJob_OpenEventSale, 
            tags: new ActivityTagsCollection(tags));

        Activity.Current?.AddEvent(e);
    }

    public void CloseEventSale(string eventId)
    {
        var tags = new KeyValuePair<string, object?>[] {
            new(__s_eventId, eventId)            
        };
        
        var e = new ActivityEvent(
            __s_EventSaleStatusUpdateJob_CloseEventSale, 
            tags: new ActivityTagsCollection(tags));

        Activity.Current?.AddEvent(e);
    }
}
