using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using MimeKit;

namespace TicketBurst.ServiceInfra.Aws;

public class AwsSesEmailGatewayPlugin : IEmailGatewayPlugin
{
    private readonly AWSCredentials _credentials;
    private readonly Amazon.RegionEndpoint _regionEndpoint;
    private readonly string _configSetName;

    public AwsSesEmailGatewayPlugin(EmailServiceSecret secret)
    {
        Console.WriteLine($"AwsSesEmailGatewayPlugin> ctor, role=[{secret.Role}], region=[{secret.Region}], configSet=[{secret.ConfigSetName}]");
        
        var instanceProfileCreds = new InstanceProfileAWSCredentials(secret.Role);
        _credentials = instanceProfileCreds;
        var tryGetCredentials = instanceProfileCreds.GetCredentials();
        Console.WriteLine($"AwsSesEmailGatewayPlugin> ctor, got credentials: token=[{tryGetCredentials.Token}], expiry=[{instanceProfileCreds.PreemptExpiryTime}]");

        _regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(secret.Region);
        Console.WriteLine($"AwsSesEmailGatewayPlugin> ctor, got region endpoint: sysname=[{_regionEndpoint.SystemName}] partition=[{_regionEndpoint.PartitionName}|{_regionEndpoint.PartitionDnsSuffix}]");

        _configSetName = secret.ConfigSetName;
    }

    public async Task SendEmailMessage(MimeMessage message)
    {
        var request = CreateSendRequest(message);
        using var client = new AmazonSimpleEmailServiceV2Client(_credentials, _regionEndpoint);

        try
        {
            await client.SendEmailAsync(request);
            Console.WriteLine($"AwsSesEmailGatewayPlugin> send to [{request.Destination.ToAddresses.First()}] SUCCESS");
        }
        catch (Exception e)
        {
            Console.WriteLine($"AwsSesEmailGatewayPlugin> send to [{request.Destination.ToAddresses.First()}] FAILED! {e.ToString()}");
        }            
    }

    private SendEmailRequest CreateSendRequest(MimeMessage message)
    {
        var toAddress = message.To.OfType<MailboxAddress>().First();
        var fromAddress = message.From.OfType<MailboxAddress>().First();

        using var rawStream = new MemoryStream();
        message.WriteTo(rawStream);
        rawStream.Flush();
        rawStream.Position = 0;

        var sendRequest = new SendEmailRequest {
            FromEmailAddress = fromAddress.ToString(encode: true),
            Content = new EmailContent() {
                Raw = new RawMessage() {
                    Data = rawStream 
                } 
            }, 
            Destination = new Destination {
                ToAddresses = new List<string> {
                    toAddress.ToString(encode: true)
                }
            },
            ConfigurationSetName = _configSetName
        };

        return sendRequest;
    }
}
