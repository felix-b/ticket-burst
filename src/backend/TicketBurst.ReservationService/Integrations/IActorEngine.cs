using TicketBurst.ReservationService.Actors;
using TicketBurst.ReservationService.Contracts;

namespace TicketBurst.ReservationService.Integrations;

public interface IActorEngine : IAsyncDisposable
{
    Task StartAsync();
    Task<IEventAreaManager?> GetActor(string eventId, string areaId);
    Task ForEachLocalActor(Func<IEventAreaManager, Task> action);
    ClusterDiagnosticInfo GetClusterDiagnostics();
}

public record ClusterDiagnosticInfo(
    string Address,
    string ClusterName,
    MemberDiagnosticInfo ThisMember,
    MemberDiagnosticInfo[] OtherMembers,
    string[] LocalGrains
);

public record MemberDiagnosticInfo(
    string Id,
    string Address,
    string Host,
    int Port
);
