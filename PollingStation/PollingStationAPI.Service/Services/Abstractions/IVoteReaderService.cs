using Azure.Storage.Blobs;
using PollingStationAPI.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollingStationAPI.Service.Services.Abstractions;

public interface IVoteReaderService
{
    Task<List<VoteBallot>> GetVotesForCandidateAsync(
        BlobServiceClient blobServiceClient,
        string containerName,
        string? targetCandidateIdentifier,      // The name or ID of the candidate you're searching for
        string? pollingStationIdFilter = null, // Optional: Filter by a specific polling station ID
        DateTime? dateFilter = null,           // Optional: Filter by a specific date
        string? atuFilter = null,              // Optional: Filter by a AUT name
        string? localityFilter = null);        // Optional: Filter by a locality name
}
