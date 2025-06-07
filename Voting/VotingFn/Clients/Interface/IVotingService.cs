using Azure.Storage.Blobs;
using VotingFn.Models;

namespace VotingFn.Clients.Interface;

public interface IVotingService
{
	Task WriteStringToBlobAsync(BlobServiceClient blobServiceClient, string containerName, string blobName, string contentToUpload);
	Task AppendStringToBlobAsync(BlobServiceClient blobServiceClient, string containerName, string blobName, string contentToUpload);
	Task<List<VoteRecord>> GetVotesForCandidateAsync( 
		BlobServiceClient blobServiceClient,
        string containerName,
        string targetCandidateIdentifier, // The name or ID of the candidate you're searching for
        string? pollingStationIdFilter = null, // Optional: Filter by a specific polling station ID
        DateTime? dateFilter = null);
}
