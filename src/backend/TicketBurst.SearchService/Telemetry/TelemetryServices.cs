using System.Diagnostics;
using TicketBurst.SearchService.Jobs;

namespace TicketBurst.SearchService.Telemetry;

public static class TelemetryServices
{
    public static readonly ActivitySource ActivitySource = new ActivitySource("ticketburst-services-search");
    
    
}
