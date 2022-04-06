using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;

namespace TicketBurst.ServiceInfra.Aws;

public class AwsSecretsManagerPlugin : ISecretsManagerPlugin
{
    private readonly SecretsManagerCache _cache = new SecretsManagerCache();
    
    public async Task<ConnectionStringSecret> GetConnectionStringSecret(string secretName)
    {
        var json = await _cache.GetSecretString(secretName);
        var secret =
            JsonSerializer.Deserialize<ConnectionStringSecret>(json)
            ?? throw new Exception($"AwsSecretsManagerPlugin: unable to retrieve secret [{secretName}]");
        return secret;
    }
}
