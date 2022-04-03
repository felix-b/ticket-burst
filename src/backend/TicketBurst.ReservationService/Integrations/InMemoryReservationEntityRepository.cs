using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations;

public class InMemoryReservationEntityRepository : IReservationEntityRepository
{
    public string MakeNewId()
    {
        return MockDatabase.MakeNewId();
    }

    public IEnumerable<ReservationJournalRecord> GetJournalEntriesForRecovery(string eventId, string areaId)
    {
        return MockDatabase.ReservationJournal.All;
    }

    public void AppendJournalEntry(ReservationJournalRecord record)
    {
        MockDatabase.ReservationJournal.Append(record);
    }
}
