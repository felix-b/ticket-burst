namespace TicketBurst.ServiceInfra;

public interface ISecretsManagerPlugin
{
    Task<ConnectionStringSecret> GetConnectionStringSecret(string secretName);
}

public record ConnectionStringSecret(
    string Host,
    int Port,
    string UserName,
    string Password
);
