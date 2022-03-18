using System.Security.Cryptography.X509Certificates;

namespace TicketBurst.Contracts;

public record SeatingMapContract(
    string Id,
    string HallAreaId,
    string Name,
    RowContract[] Rows
);
