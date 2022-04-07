using System.Net;
using Amazon.S3.Model;

namespace TicketBurst.ServiceInfra.Aws;

using Amazon.S3;

public class AwsS3StorageGatewayPlugin : IStorageGatewayPlugin
{
    private readonly string _bucketArn;
    
    public AwsS3StorageGatewayPlugin()
    {
        _bucketArn = 
            Environment.GetEnvironmentVariable("AWS_S3_BUCKET_ARN") 
            ?? throw new Exception("AwsS3StorageGatewayPlugin: missing env var 'AWS_S3_BUCKET_ARN'");
    }

    public async Task UploadObject(string objectKey, byte[] contents, string contentType, IDictionary<string, string>? metadata = null)
    {
        using var s3Client = new AmazonS3Client();
        using var inputStream = new MemoryStream(contents);
        
        var putRequest = new PutObjectRequest {
            BucketName = _bucketArn,
            Key = objectKey,
            InputStream = inputStream,
            AutoCloseStream = false,
            AutoResetStreamPosition = false
        };

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                putRequest.Metadata.Add(kvp.Key, kvp.Value);
            }
        }

        var response = await s3Client.PutObjectAsync(putRequest);
        if (response.HttpStatusCode >= HttpStatusCode.BadRequest)
        {
            throw new Exception(
                $"S3 upload FAILED! HTTP {(int)response.HttpStatusCode}. Bucket=[{_bucketArn}] Key=[{objectKey}");
        }
    }

    public Task<byte[]> DownloadObject(string objectKey)
    {
        throw new NotImplementedException();
    }
}
