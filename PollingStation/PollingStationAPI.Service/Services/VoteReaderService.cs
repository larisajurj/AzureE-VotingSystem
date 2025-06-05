using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using PollingStationAPI.Service.Services.Abstractions;
using System.Text.Json;
using PollingStationAPI.Service.DTOs;

namespace PollingStationAPI.Service.Service;

public class VoteReaderService : IVoteReaderService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public async Task<List<VoteBallot>> GetVotesForCandidateAsync(
    BlobServiceClient blobServiceClient,
    string containerName,
    string? targetCandidateIdentifier, // The name or ID of the candidate you're searching for
    string? pollingStationIdFilter = null, // Optional: Filter by a specific polling station ID
    DateTime? dateFilter = null,          // Optional: Filter by a specific date
    string? atuFilter = null,
    string? localityFilter = null)
    {
        var matchingVotes = new List<VoteBallot>();
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
                // This extra check is still useful if a date isn't provided but a station is
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
                                VoteBallot? voteRecord = JsonSerializer.Deserialize<VoteBallot>(line, _jsonSerializerOptions);

                                // --- MODIFIED SECTION: Enhanced Filtering Logic ---
                                if (voteRecord != null)
                                {
                                    // Check if the record matches all provided filters
                                    bool isCandidateMatch = string.IsNullOrEmpty(targetCandidateIdentifier) || voteRecord.CandidateVoted.Equals(targetCandidateIdentifier, StringComparison.OrdinalIgnoreCase);

                                    // If a filter is null/empty, it's considered a match for that criteria
                                    bool isAtuMatch = string.IsNullOrEmpty(atuFilter) ||
                                                      voteRecord.PollingStation.ATU.Equals(atuFilter, StringComparison.OrdinalIgnoreCase);

                                    bool isLocalityMatch = string.IsNullOrEmpty(localityFilter) ||
                                                           voteRecord.PollingStation.Locality.Equals(localityFilter, StringComparison.OrdinalIgnoreCase);

                                    if (isCandidateMatch && isAtuMatch && isLocalityMatch)
                                    {
                                        matchingVotes.Add(voteRecord);
                                    }
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
