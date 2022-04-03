using System.Collections.Immutable;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Integrations;
using TicketBurst.ServiceInfra;
using Timer = System.Threading.Timer;

namespace TicketBurst.SearchService.Jobs;

public class EventSaleStatusUpdateJob : IDisposable
{
    private readonly ISearchEntityRepository _entityRepo;
    private readonly IMessagePublisher<EventSaleNotificationContract> _publisher;
    private readonly Timer _timer;

    public EventSaleStatusUpdateJob(
        ISearchEntityRepository entityRepo,
        IMessagePublisher<EventSaleNotificationContract> publisher)
    {
        _entityRepo = entityRepo;
        _publisher = publisher;
        _timer = new Timer(
            InProcessJob.WithTimerErrorHandling(this, HandleTimerTick), 
            state: null, 
            dueTime: TimeSpan.FromSeconds(15), 
            period: TimeSpan.FromSeconds(15));
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private void HandleTimerTick()
    {
        var now = DateTime.UtcNow;
  
        foreach (var @event in _entityRepo.GetAllEventsSync())
        {
            try
            {
                ProcessEvent(@event);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(EventSaleStatusUpdateJob)}: failed to process event [{@event.Id}]: {e}");
            }
        }

        void ProcessEvent(EventContract @event)
        {
            if (ShouldOpenForSale(@event))
            {
                OpenEventForSale(@event);
            }
            else if (ShouldCloseForSale(@event))
            {
                CloseEventForSale(@event);
            }
        }

        void OpenEventForSale(EventContract @event)
        {
            Console.WriteLine($"{nameof(EventSaleStatusUpdateJob)}: opening sale of event [{@event.Id}]");
            _entityRepo.UpdateIsOpenForSale(@event.Id, true).Wait();

            var notification = CreateOpenSaleNotification(@event);
            _publisher.Publish(notification);
        }

        void CloseEventForSale(EventContract @event)
        {
            Console.WriteLine($"{nameof(EventSaleStatusUpdateJob)}: closing sale of event [{@event.Id}]");
            _entityRepo.UpdateIsOpenForSale(@event.Id, false).Wait();
        }

        bool ShouldOpenForSale(EventContract @event)
        {
            return (
                !@event.IsOpenForSale &&
                @event.SaleStartUtc <= now && 
                now <= @event.EventStartUtc + TimeSpan.FromMinutes(30));
        }

        bool ShouldCloseForSale(EventContract @event)
        {
            return (
                @event.IsOpenForSale &&
                now > @event.EventStartUtc + TimeSpan.FromMinutes(30));
        }

        EventSaleNotificationContract CreateOpenSaleNotification(EventContract @event)
        {
            var hallSeatingMap = _entityRepo.GetHallSeatingMapByIdOrThrowSync(@event.HallSeatingMapId);
            var hallAreaIds = hallSeatingMap
                .Areas.Select(a => a.HallAreaId)
                .ToImmutableList();

            return new EventSaleNotificationContract(
                Id: _entityRepo.MakeNewId(),
                PublishedAtUtc: DateTime.UtcNow,
                EventId: @event.Id,
                HallAreaIds: hallAreaIds,
                SaleStartUtc: @event.SaleStartUtc);
        }
    }
}
