using System.Collections.Concurrent;

namespace TicketBurst.ReservationService;

public static class MockDatabase
{
    public static class ReservationJournal
    {
        private static readonly ConcurrentBag<ReservationJournalRecord> __all = new();

        public static IReadOnlyCollection<ReservationJournalRecord> All => __all;

        public static void Append(ReservationJournalRecord record)
        {
            __all.Add(record);
        }
    }
}
