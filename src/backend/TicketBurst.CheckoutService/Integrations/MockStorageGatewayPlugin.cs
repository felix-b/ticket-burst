using System.Reflection;

namespace TicketBurst.CheckoutService.Integrations;

public class MockStorageGatewayPlugin : IStorageGatewayPlugin
{
    private static readonly string __storageFolderPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
        "mock-storage");
    
    public MockStorageGatewayPlugin()
    {
        Directory.CreateDirectory(__storageFolderPath);
    }

    public async Task UploadObject(string objectKey, byte[] contents)
    {
        var filePath = Path.Combine(
            __storageFolderPath,
            objectKey.Replace("/", "__"));
        
        await File.WriteAllBytesAsync(filePath, contents);
    }

    public async Task<byte[]> DownloadObject(string objectKey)
    {
        var filePath = Path.Combine(
            __storageFolderPath,
            objectKey.Replace("/", "__"));
        
        return await File.ReadAllBytesAsync(filePath);
    }
}
