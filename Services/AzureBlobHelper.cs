using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

public class AzureBlobHelper
{
    private readonly BlobContainerClient _container;

    public AzureBlobHelper(IConfiguration config)
    {
        var connStr = config["AzureBlob:ConnectionString"];
        var containerName = config["AzureBlob:ContainerName"];
        _container = new BlobContainerClient(connStr, containerName);
    }

    public async Task DeleteBlobIfExistsAsync(string blobUrl)
    {
        if (string.IsNullOrEmpty(blobUrl)) return;

        var uri = new Uri(blobUrl);
        var blobName = Path.GetFileName(uri.LocalPath);
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync();
    }
}
