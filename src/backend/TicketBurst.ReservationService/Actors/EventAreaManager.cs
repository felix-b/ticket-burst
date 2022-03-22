using System.Net;
using TicketBurst.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Actors;

public class EventAreaManager
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, SeatEntry> _seatById = new();

    public EventAreaManager(string eventId, string areaId)
    {
        EventId = eventId;
        AreaId = areaId;
    }

    public async Task RecoverState()
    {
        var map = await ServiceClient.HttpGetJson<AreaSeatingMapContract>(
            serviceName: "search",
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

        foreach (var row in map.Rows)
        {
            foreach (var seat in row.Seats)
            {
                var lastJournalRecord = lastJournalRecordBySeatId.TryGetValue(seat.Id, out var journalRecord)
                    ? journalRecord
                    : null;
                _seatById[seat.Id] = new SeatEntry(row, seat, lastJournalRecord);
            }
        }
    }

    public SeatReservationReplyContract TryReserveSeats(SeatReservationRequestContract request)
    {
        lock (_syncRoot)
        {
            return new SeatReservationReplyContract(
                Request: request,
                Success: false,
                ReservationId: null,
                ReservationExpiryUtc: null,
                ErrorCode: "NotImplemented");
        }
    }
    
    public string EventId { get;  }
    public string AreaId { get;  }
    
    public record SeatEntry(
        SeatingMapRowContract Row,
        SeatingMapSeatContract Seat,
        ReservationJournalRecord? LastAudit
    );
}
