using Azure.Storage.Blobs;

namespace VotingFn.Factory.Interface;

public interface IBlobServiceClientFactory
{
	BlobServiceClient GetClient();
}
