using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using Azure.Storage.Blobs;
using VotingFn.Factory.Interface;
using VotingFn.Clients.Interface;
using VotingFn.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace VotingFn;

public class VotingFunction
{
	private readonly ILogger _logger;
	private readonly IBlobServiceClientFactory _blobServiceClientFactory;
	private readonly IVotingService _votingService;
	public VotingFunction(ILoggerFactory loggerFactory, IBlobServiceClientFactory blobServiceClientFactory, IVotingService votingService)
	{
		_logger = loggerFactory.CreateLogger<VotingFunction>();
		_blobServiceClientFactory = blobServiceClientFactory;
		_votingService = votingService;
	}

	[Function("SendVote")]
	public async Task<IActionResult> SendVote(
	[HttpTrigger(AuthorizationLevel.Anonymous, WebRequestMethods.Http.Post, Route = "SendVote")] HttpRequestData req,
	CancellationToken cancellationToken)
	{
		string requestBody;

		using (var reader = new System.IO.StreamReader(req.Body))
		{
			requestBody = await reader.ReadToEndAsync();
			_logger.LogDebug("Request Body: {Body}", requestBody); // Log raw body if needed (be careful with sensitive data)
		}


		if (string.IsNullOrEmpty(requestBody))
		{
			_logger.LogWarning("Request body was null or empty.");
			return new BadRequestResult();
		}

		var vote = JsonSerializer.Deserialize<VoteRecord>(
			requestBody,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (vote == null)
		{
			_logger.LogWarning("Failed to deserialize request body into VoteRecord. Body content: {Body}", requestBody);
			return new BadRequestResult();
		}
		BlobServiceClient serviceClient = _blobServiceClientFactory.GetClient(); // Get the client from your factory

		string filename = $"log-{DateTime.UtcNow:yyyyMMddHHmmssfff}.txt";
		string serializeVote = JsonSerializer.Serialize(vote) + "\n";

		await _votingService.AppendStringToBlobAsync(serviceClient, "votes-container", vote.GetBlobPath(vote), serializeVote);
		return new OkObjectResult("Successfully registered");
	}
}
