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
        
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new Exception($"AwsSecretsManagerPlugin: unable to retrieve connstr secret [{secretName}]");
        }
        
        var secret =
            JsonSerializer.Deserialize<ConnectionStringSecret>(json)
            ?? throw new Exception($"AwsSecretsManagerPlugin: unable to parse connstr secret [{secretName}]");
        
        Console.WriteLine($"AwsSecretsManagerPlugin> retrieved connstr secret [{secretName}], json=[{json}]");
        Console.WriteLine($"AwsSecretsManagerPlugin> retrieved connstr secret [{secretName}], server=[{secret.Server}] port=[${secret.Port}]");

        return secret;
    }
}
