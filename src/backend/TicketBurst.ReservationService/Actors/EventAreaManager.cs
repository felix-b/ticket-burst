using System.Collections.Immutable;
using System.Net;
using TicketBurst.Contracts;
using TicketBurst.ReservationService.Contracts;
using TicketBurst.ReservationService.Integrations;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Actors;

public class EventAreaManager
{
    public const int TemporaryReservationExpiryMinutes = 7;
    
    private readonly object _syncRoot = new(); //TODO: get rid once integrated with actor framework
    private readonly IReservationEntityRepository _entityRepo;
    private ImmutableDictionary<string, SeatEntry> _seatById = ImmutableDictionary<string, SeatEntry>.Empty;
    private string _seatingMapId = string.Empty;
    private ulong _lastJournalSequenceNo = 1;

    public EventAreaManager(string eventId, string areaId, IReservationEntityRepository entityRepo)
    {
        EventId = eventId;
        AreaId = areaId;

        _entityRepo = entityRepo;
    }

    public async Task RecoverState()
    {
        var map = await ServiceClient.HttpGetJson<AreaSeatingMapContract>(
            ServiceName.Search,
            path: new[] { "event", EventId, "area-seatmap", AreaId });

        if (map == null)
        {
            throw new Exception(
                $"Failed to query event area seating map ({EventId}/{AreaId}) from Search service");
        }

        var lastJournalRecordBySeatId = new Dictionary<string, ReservationJournalRecord>();
        foreach (var record in _entityRepo.GetJournalEntriesForRecovery(EventId, AreaId))
        {
            foreach (var seatId in record.SeatIds)
            {
                lastJournalRecordBySeatId[seatId] = record;
            }
        }

        var seatByIdBuilder = ImmutableDictionary.CreateBuilder<string, SeatEntry>();
        
        foreach (var row in map.Rows)
        {
            foreach (var seat in row.Seats)
            {
                var lastJournalRecord = lastJournalRecordBySeatId.TryGetValue(seat.Id, out var journalRecord)
                    ? journalRecord
                    : null;
                seatByIdBuilder[seat.Id] = new SeatEntry(row, seat, lastJournalRecord);
            }
        }

        _seatById = seatByIdBuilder.ToImmutable();
        _seatingMapId = map.SeatingMapId;
    }

    public SeatReservationReplyContract TryReserveSeats(SeatReservationRequestContract request)
    {
        lock (_syncRoot)
        {
            var seats = new List<SeatEntry>();
            
            foreach (var seatId in request.SeatIds)
            {
                if (!_seatById.TryGetValue(seatId, out var seat))
                {
                    return SeatReservationReplyContract.FromError(request, "SeatNotFound");
                }
                
                if (seat.Status != SeatStatus.Available)
                {
                    return SeatReservationReplyContract.FromError(request, "SeatNotAvailable");
                }
            
                seats.Add(seat);
            }

            var record = new ReservationJournalRecord(
                Id: _entityRepo.MakeNewId(),
                CreatedAtUtc: DateTime.UtcNow,
                EventId: EventId,
                HallAreaId: AreaId,
                HallSeatingMapId: _seatingMapId,
                SequenceNo: Interlocked.Increment(ref _lastJournalSequenceNo),
                SeatIds: request.SeatIds.ToImmutableList(),
                Action: ReservationAction.TemporarilyReserve,
                ResultStatus: SeatStatus.Reserved);
            ApplyJournalRecord(record);            

            // MockDatabase.ReservationJournal.Append(record);
            // var seatByIdMutation = _seatById.ToBuilder();
            // seats.ForEach(entry => seatByIdMutation[entry.Seat.Id] = entry with { LastRecord = record});
            // _seatById = seatByIdMutation.ToImmutable();
            
            return new SeatReservationReplyContract(
                Request: request,
                Success: true,
                CheckoutToken: record.Id,
                ReservationExpiryUtc: record.CreatedAtUtc.AddMinutes(TemporaryReservationExpiryMinutes));
        }
    }

    public EventAreaUpdateNotificationContract GetUpdateNotification()
    {
        var snapshot = _seatById;
        var totalCapacity = snapshot.Count;
        var availableCapacity = snapshot.Values.Count(seat => seat.Status == SeatStatus.Available);

        return new EventAreaUpdateNotificationContract(
            Id: _entityRepo.MakeNewId(),
            SequenceNo: _lastJournalSequenceNo,
            PublishedAtUtc: DateTime.UtcNow,
            EventId: EventId,
            HallAreaId: AreaId,
            TotalCapacity: totalCapacity,
            AvailableCapacity: availableCapacity,
            StatusBySeatId: snapshot.Values.ToImmutableDictionary(
                keySelector: e => e.Seat.Id, 
                elementSelector: e => e.Status));
    }

    public void ReleaseExpiredReservations()
    {
        var now = DateTime.UtcNow;
        var isReservationExpired = (SeatEntry entry) =>
            now.Subtract(entry.LastRecord!.CreatedAtUtc).TotalMinutes >= TemporaryReservationExpiryMinutes;

        lock (_syncRoot)
        {
            var entriesToRelease = _seatById.Values
                .Where(entry => entry.LastRecord?.Action == ReservationAction.TemporarilyReserve)
                .Where(isReservationExpired)
                .ToList();
            
            var record = new ReservationJournalRecord(
                Id: _entityRepo.MakeNewId(),
                CreatedAtUtc: now,
                EventId: EventId,
                HallAreaId: AreaId,
                HallSeatingMapId: _seatingMapId,
                SequenceNo: Interlocked.Increment(ref _lastJournalSequenceNo),
                SeatIds: entriesToRelease.Select(entry => entry.Seat.Id).ToImmutableList(),
                Action: ReservationAction.ReleasePerTimeout,
                ResultStatus: SeatStatus.Available);
            ApplyJournalRecord(record);

            entriesToRelease.ForEach(entry => {
                Console.WriteLine($"EAM[{EventId}/{AreaId}]: seat[{entry.Seat.Id}] temporary reservation expired, released.");
            });
        }
    }

    public ReservationJournalRecord? FindEffectiveJournalRecordById(string reservationId)
    {
        lock (_syncRoot)
        {
            foreach (var seat in _seatById.Values)
            {
                if (seat.LastRecord?.Id == reservationId)
                {
                    return seat.LastRecord;
                }
            }

            return null;
        }
    }

    public bool UpdateReservationPerOrderStatus(string reservationId, uint orderNumber, OrderStatus orderStatus)
    {
        var effectiveRecord = FindEffectiveJournalRecordById(reservationId);
        if (effectiveRecord == null || 
            effectiveRecord.Action != ReservationAction.TemporarilyReserve || 
            effectiveRecord.ResultStatus != SeatStatus.Reserved)
        {
            return false;
        }

        var newRecord = effectiveRecord with {
            Id = _entityRepo.MakeNewId(),
            SequenceNo = Interlocked.Increment(ref _lastJournalSequenceNo),
            CreatedAtUtc = DateTime.UtcNow,
            OrderNumber = orderNumber,
            Action = orderStatus == OrderStatus.Completed  
                ? ReservationAction.PermanentlyReservePerOrderCompleted
                : ReservationAction.ReleasePerOrderCanceled,
            ResultStatus = orderStatus == OrderStatus.Completed  
                ? SeatStatus.Sold
                : SeatStatus.Available
        };
        
        ApplyJournalRecord(newRecord);
        return true;
    }
    
    public string EventId { get; }
    public string AreaId { get; }

    private void ApplyJournalRecord(ReservationJournalRecord record)
    {
        lock (_syncRoot)
        {
            var mutation = _seatById.ToBuilder();

            foreach (var seatId in record.SeatIds)
            {
                mutation[seatId] = mutation[seatId] with {
                    LastRecord = record
                };
            }

            _entityRepo.AppendJournalEntry(record);
            _seatById = mutation.ToImmutable();
        }
    }

    public record SeatEntry(
        SeatingMapRowContract Row,
        SeatingMapSeatContract Seat,
        ReservationJournalRecord? LastRecord)
    {
        public SeatStatus Status =>
            LastRecord?.ResultStatus ?? SeatStatus.Available;
    }
}
