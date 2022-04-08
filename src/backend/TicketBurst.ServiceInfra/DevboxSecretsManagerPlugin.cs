#pragma warning disable CS1998

namespace TicketBurst.ServiceInfra;

public class DevboxSecretsManagerPlugin : ISecretsManagerPlugin
{
    public async Task<ConnectionStringSecret> GetConnectionStringSecret(string secretName)
    {
        switch (secretName)
        {
            case "checkout-db-connstr":
                return new ConnectionStringSecret(
                    Host: "localhost", 
                    Port: 3306, 
                    UserName: "root", 
                    Password: "rootpass1"
                );
            case "reservation-db-connstr":
            case "search-db-connstr":
                return new ConnectionStringSecret(
                    Host: string.Empty, 
                    Port: 0, 
                    UserName: string.Empty, 
                    Password: string.Empty
                );
            default:
                throw new ArgumentException("Unknown secret", paramName: nameof(secretName));
        }
    }

    public Task<EmailServiceSecret> GetEmailServiceSecret(string secretName)
    {
        throw new NotSupportedException();
    }
}
