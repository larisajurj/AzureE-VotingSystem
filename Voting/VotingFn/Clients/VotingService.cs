using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.Text;
using System.Text.Json;
using VotingFn.Clients.Interface;
using VotingFn.Models;

namespace VotingFn.Clients;

public class VotingService : IVotingService
{
	private readonly JsonSerializerOptions _jsonSerializerOptions =
		new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

	public async Task WriteStringToBlobAsync(
		BlobServiceClient blobServiceClient,
		string containerName,
		string blobName,
		string contentToUpload)
	{
		try
		{
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

			BlobClient blobClient = containerClient.GetBlobClient(blobName);

			Console.WriteLine($"Got client for blob '{blobName}' in container '{containerName}'.");

			byte[] byteArray = Encoding.UTF8.GetBytes(contentToUpload);
			using (MemoryStream stream = new MemoryStream(byteArray))
			{
				Console.WriteLine($"Uploading data to {blobClient.Uri}...");

				await blobClient.UploadAsync(stream, overwrite: true);

				Console.WriteLine($"Successfully uploaded blob '{blobName}'.");
			}
		}
		catch (Azure.RequestFailedException ex)
		{
			Console.Error.WriteLine($"Error uploading blob: {ex.Message}");
			Console.Error.WriteLine($"Status Code: {ex.Status}");
			Console.Error.WriteLine($"Error Code: {ex.ErrorCode}");
			throw;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
			throw;
		}
	}

	public async Task AppendStringToBlobAsync(
		BlobServiceClient blobServiceClient,
		string containerName,
		string blobPath,
		string contentToUpload)
	{
		try
		{
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
			await containerClient.CreateIfNotExistsAsync();

			AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobPath);

			Console.WriteLine($"Got AppendBlobClient for blob '{blobPath}' in container '{containerName}'.");

			// Create the append blob if it doesn't exist
			await appendBlobClient.CreateIfNotExistsAsync();

			byte[] byteArray = Encoding.UTF8.GetBytes(contentToUpload.ToString());
			using (MemoryStream stream = new MemoryStream(byteArray))
			{
				Console.WriteLine($"Appending data to {appendBlobClient.Uri}...");

				await appendBlobClient.AppendBlockAsync(stream);

				Console.WriteLine($"Successfully appended to blob '{blobPath}'.");
			}
		}
		catch (Azure.RequestFailedException ex)
		{
			Console.Error.WriteLine($"Error uploading to append blob: {ex.Message}");
			Console.Error.WriteLine($"Status Code: {ex.Status}");
			Console.Error.WriteLine($"Error Code: {ex.ErrorCode}");
			throw;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
			throw;
		}
	}

	public async Task<List<VoteRecord>> ReadVotesForDayAsync(
	BlobServiceClient blobServiceClient,
	string containerName,
	DateTime date)
	{
		var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
		string prefix = $"raw/year={date:yyyy}/month={date:MM}/day={date:dd}/";

		var voteRecords = new List<VoteRecord>();

		await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: prefix))
		{
			BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);

			using var stream = await blobClient.OpenReadAsync();
			using var reader = new StreamReader(stream);

			while (!reader.EndOfStream)
			{
				string? line = await reader.ReadLineAsync();
				if (!string.IsNullOrWhiteSpace(line))
				{
					try
					{
						var vote = JsonSerializer.Deserialize<VoteRecord>(line, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						});
						if (vote != null)
							voteRecords.Add(vote);
					}
					catch (JsonException ex)
					{
						Console.Error.WriteLine($"Failed to deserialize line: {ex.Message}");
					}
				}
			}
		}

		return voteRecords;
	}

	public async Task<List<VoteRecord>> GetVotesForCandidateAsync(
        BlobServiceClient blobServiceClient,
        string containerName,
        string targetCandidateIdentifier, // The name or ID of the candidate you're searching for
        string? pollingStationIdFilter = null, // Optional: Filter by a specific polling station ID
        DateTime? dateFilter = null)          // Optional: Filter by a specific date
    {
        var matchingVotes = new List<VoteRecord>();
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        string prefix = "raw/";
        if (dateFilter.HasValue)
        {
            prefix += $"year={dateFilter.Value:yyyy}/month={dateFilter.Value:MM}/day={dateFilter.Value:dd}/";
            if (!string.IsNullOrEmpty(pollingStationIdFilter))
            {
                prefix += $"station={pollingStationIdFilter}/";
            }
        }

        Console.WriteLine($"Listing blobs with prefix: '{prefix}'");

        try
        {
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                if (!string.IsNullOrEmpty(pollingStationIdFilter) && !blobItem.Name.Contains($"station={pollingStationIdFilter}/"))
                {
                    continue;
                }

                if (!blobItem.Name.EndsWith(".jsonl") || !blobItem.Name.Contains("votes_"))
                {
                    continue;
                }

                Console.WriteLine($"Processing blob: {blobItem.Name}");
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);

                try
                {
                    BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                    string content = downloadResult.Content.ToString();

                    using (StringReader stringReader = new StringReader(content))
                    {
                        string? line;
                        while ((line = await stringReader.ReadLineAsync()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            try
                            {
                                VoteRecord? voteRecord = JsonSerializer.Deserialize<VoteRecord>(line, _jsonSerializerOptions);

                                if (voteRecord != null &&
                                    voteRecord.CandidateVoted.Equals(targetCandidateIdentifier, StringComparison.OrdinalIgnoreCase)) // Adjust comparison as needed
                                {
                                    matchingVotes.Add(voteRecord);
                                }
                            }
                            catch (JsonException jsonEx)
                            {
                                Console.Error.WriteLine($"Error deserializing line from blob {blobItem.Name}: {jsonEx.Message}. Line: '{line.Substring(0, Math.Min(line.Length, 100))}'");
                            }
                        }
                    }
                }
                catch (RequestFailedException rfEx)
                {
                    Console.Error.WriteLine($"Error downloading/accessing blob {blobItem.Name}: {rfEx.Message} (Status: {rfEx.Status})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred while listing or processing blobs: {ex.Message}");
            throw;
        }


        return matchingVotes;
    }


}
