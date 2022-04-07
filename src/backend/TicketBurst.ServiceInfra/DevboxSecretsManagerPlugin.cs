#pragma warning disable CS1998

namespace TicketBurst.ServiceInfra;

public class DevboxSecretsManagerPlugin : ISecretsManagerPlugin
{
    public async Task<ConnectionStringSecret> GetConnectionStringSecret(string secretName)
    {
        switch (secretName)
        {
            case "checkout-db-connstr":
                return new ConnectionStringSecret(Server: "localhost", Port: 3306, UserName: "root", Password: "rootpass1");
            default:
                throw new ArgumentException("Unknown secret", paramName: nameof(secretName));
        }
    }
}
