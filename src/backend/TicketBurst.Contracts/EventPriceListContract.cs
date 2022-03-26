using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record EventPriceListContract(
    ImmutableDictionary<string, decimal> PriceByLevelId
);

