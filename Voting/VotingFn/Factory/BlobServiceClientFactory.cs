using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using VotingFn.Factory.Interface;

namespace VotingFn.Factory;

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
