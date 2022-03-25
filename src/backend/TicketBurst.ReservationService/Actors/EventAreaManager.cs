using System.Collections.Immutable;
using System.Net;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Actors;

public class EventAreaManager
{
    public const int TemporaryReservationExpiryMinutes = 7;
    
    private readonly object _syncRoot = new();
    private ImmutableDictionary<string, SeatEntry> _seatById = ImmutableDictionary<string, SeatEntry>.Empty;
    private string _seatingMapId = string.Empty;
    private ulong _lastJournalSequenceNo = 1;

    public EventAreaManager(string eventId, string areaId)
    {
        EventId = eventId;
        AreaId = areaId;
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
        foreach (var record in MockDatabase.ReservationJournal.All)
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
                Id: MockDatabase.MakeNewId(),
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
            Id: MockDatabase.MakeNewId(),
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
                Id: MockDatabase.MakeNewId(),
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

            MockDatabase.ReservationJournal.Append(record);
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
