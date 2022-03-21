namespace TicketBurst.Contracts;

public record ServerInfoContract(
    string? HostName,
    string? PodName,
    string? PodIPAddress,
    string? PodServiceAccount,
    string? ShardId
);
