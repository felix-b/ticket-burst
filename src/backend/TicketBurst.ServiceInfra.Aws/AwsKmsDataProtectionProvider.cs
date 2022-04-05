using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Microsoft.AspNetCore.DataProtection;

namespace TicketBurst.ServiceInfra.Aws;

public class AwsKmsDataProtectionProvider : IDataProtectionProvider, IDataProtector
{
    private readonly string _cmkArn;
    private AmazonKeyManagementServiceClient _kmsClient;

    public AwsKmsDataProtectionProvider()
    {
        _cmkArn = Environment.GetEnvironmentVariable("AWS_KMS_CMK_ARN") ?? string.Empty;
        _kmsClient = new AmazonKeyManagementServiceClient();
    }

    public IDataProtector CreateProtector(string purpose)
    {
        return this;
    }

    public byte[] Protect(byte[] plaintext)
    {
        MemoryStream plaintextStream = new MemoryStream();
        plaintextStream.Write(plaintext, 0, plaintext.Length);

        EncryptRequest encryptRequest = new EncryptRequest() {
            KeyId = _cmkArn,
            Plaintext = plaintextStream
        };
        
        var task = _kmsClient.EncryptAsync(encryptRequest);
        task.Wait();
        MemoryStream ciphertext = task.Result.CiphertextBlob;
        return ciphertext.ToArray();
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        MemoryStream cipherStream = new MemoryStream();
        cipherStream.Write(protectedData);

        DecryptRequest  decryptRequest = new DecryptRequest () {
            KeyId = _cmkArn,
            CiphertextBlob = cipherStream
        };
        
        var task = _kmsClient.DecryptAsync(decryptRequest);
        task.Wait();
        MemoryStream plainText = task.Result.Plaintext;
        return plainText.ToArray();
    }
}
