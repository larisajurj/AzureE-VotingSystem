using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using PollingStationAPI.Service.DTOs;
using PollingStationAPI.Service.Factories;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ResultsController : ControllerBase
{
    private readonly IVoteReaderService _voteReaderService;
    private readonly IBlobServiceClientFactory _blobServiceClientFactory;

    public ResultsController(IVoteReaderService voteReaderService, IBlobServiceClientFactory blobServiceClientFactory)
    {
        this._voteReaderService = voteReaderService;
        this._blobServiceClientFactory = blobServiceClientFactory;
    }

    [HttpPost()]
    public async Task<IActionResult> GetResults([FromBody] VoteIdentifier vote)
    {
        if (vote == null)
        {
            return new BadRequestResult();
        }

        BlobServiceClient serviceClient = _blobServiceClientFactory.GetClient();

        var count = await _voteReaderService.GetVotesForCandidateAsync(serviceClient, "votes-container", vote.targetCandidateIdentifier, vote.pollingStationIdFilter, vote.dateFilter, vote.ATU, vote.Locality);
        return new OkObjectResult(count);
    }

}
