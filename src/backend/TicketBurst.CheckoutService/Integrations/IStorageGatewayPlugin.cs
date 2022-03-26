namespace TicketBurst.CheckoutService.Integrations;

public interface IStorageGatewayPlugin
{
    Task UploadObject(string objectKey, byte[] contents);
    Task<byte[]> DownloadObject(string objectKey);
}
