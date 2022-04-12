namespace TicketBurst.ServiceInfra;

public interface ISecretsManagerPlugin
{
    Task<ConnectionStringSecret> GetConnectionStringSecret(string secretName);
    Task<EmailServiceSecret> GetEmailServiceSecret(string secretName);
}

public record ConnectionStringSecret(
    string Host,
    int Port,
    string UserName,
    string Password
);

public record EmailServiceSecret(
    string Role,
    string Region,
    string ConfigSetName,
    string CheckoutStateMachineArn
);
