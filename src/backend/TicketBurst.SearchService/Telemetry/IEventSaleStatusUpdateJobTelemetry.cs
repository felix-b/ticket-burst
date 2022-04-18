using System.Diagnostics;
using TicketBurst.ServiceInfra;

namespace TicketBurst.SearchService.Telemetry;

public interface IEventSaleStatusJobTelemetry
{
    void OpenEventSale(string eventId);
    void CloseEventSale(string eventId);
    ITelemetrySpan NotifyOpenEventSale(string eventId);
    ITelemetrySpan NotifyCloseEventSale(string eventId);
}
