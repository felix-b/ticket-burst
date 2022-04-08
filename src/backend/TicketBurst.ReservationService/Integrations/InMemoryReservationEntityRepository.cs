using TicketBurst.ReservationService.Contracts;
using TicketBurst.ServiceInfra;

namespace TicketBurst.ReservationService.Integrations;

public class InMemoryReservationEntityRepository : IReservationEntityRepository
{
    public string MakeNewId()
    {
        return MockDatabase.MakeNewId();
    }

    public IAsyncEnumerable<ReservationJournalRecord> GetJournalEntriesForRecovery(string eventId, string areaId)
    {
        return MockDatabase.ReservationJournal.All.ToAsyncEnumerable();
    }

    public Task AppendJournalEntry(ReservationJournalRecord record)
    {
        MockDatabase.ReservationJournal.Append(record);
        return Task.CompletedTask;
    }
}
