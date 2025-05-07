using Azure.Storage.Blobs;

namespace VotingFn.Clients.Interface;

public interface IVotingService
{
	Task WriteStringToBlobAsync(BlobServiceClient blobServiceClient, string containerName, string blobName, string contentToUpload);
	Task AppendStringToBlobAsync(BlobServiceClient blobServiceClient, string containerName, string blobName, string contentToUpload);
}
