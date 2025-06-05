using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.Extensions.Configuration;

namespace PollingStationAPI.Service.Factories;

public interface IBlobServiceClientFactory
{
    BlobServiceClient GetClient();
}

public class BlobServiceClientFactory : IBlobServiceClientFactory
{
    private readonly BlobServiceClient _client;

    public BlobServiceClientFactory(IConfiguration configuration)
    {
        string accountName = configuration["AzureBlob:AccountName"];
        string accountKey = configuration["AzureBlob:AccountKey"];

        StorageSharedKeyCredential sharedKeyCredential =
             new StorageSharedKeyCredential(accountName, accountKey);

        string blobUri = "https://" + accountName + ".blob.core.windows.net";

        _client = new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
    }

    public BlobServiceClient GetClient()
    {
        return _client;
    }
}
