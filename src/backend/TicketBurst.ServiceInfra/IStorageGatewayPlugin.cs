namespace TicketBurst.ServiceInfra;

public interface IStorageGatewayPlugin
{
    Task UploadObject(string objectKey, byte[] contents, string contentType, IDictionary<string, string>? metadata = null);
    Task<byte[]> DownloadObject(string objectKey);
}
