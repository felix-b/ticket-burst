using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations;

public interface IReservationEntityRepository
{
    string MakeNewId();
    IAsyncEnumerable<ReservationJournalRecord> GetJournalEntriesForRecovery(string eventId, string areaId);
    Task AppendJournalEntry(ReservationJournalRecord @record);
}
