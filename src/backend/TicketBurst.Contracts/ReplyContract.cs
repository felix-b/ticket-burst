using System.Collections.Immutable;

namespace TicketBurst.Contracts;

public record ReplyContract<T>(
    T? Data,
    ImmutableList<ServerInfoContract>? ServerInfo
);
