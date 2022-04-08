namespace TicketBurst.ServiceInfra;

public interface IClusterInfoProvider
{
    string GetMemberUrl(int memberIndex);
    ClusterInfo Current { get; }
    event Action Changed;
}

public record ClusterInfo(
    int MemberCount,
    int ThisMemberIndex,
    int Generation
);  
