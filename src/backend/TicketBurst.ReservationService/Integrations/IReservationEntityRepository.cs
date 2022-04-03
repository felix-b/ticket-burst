using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations;

public interface IReservationEntityRepository
{
    string MakeNewId();
    IEnumerable<ReservationJournalRecord> GetJournalEntriesForRecovery(string eventId, string areaId);
    void AppendJournalEntry(ReservationJournalRecord @record);
}
