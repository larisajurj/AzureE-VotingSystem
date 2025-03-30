using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockchainAPI.Controllers;


[Route("api/vote")]
[ApiController]
public class VotingController : ControllerBase
{
	private readonly IConfiguration _configuration;
	private readonly Web3 _web3;
	private readonly string _contractAddress;
	private readonly string _privateKey;
	private readonly string _accountAddress;

	public VotingController(IConfiguration configuration)
	{
		_configuration = configuration;

		// Load blockchain settings
		var alchemyApiKey = _configuration["Blockchain:API_KEY"];
		_privateKey = _configuration["Blockchain:PRIVATE_KEY"];
		_contractAddress = _configuration["Blockchain:CONTRACT_ADDRESS"];

		var account = new Account(_privateKey);
		_accountAddress = account.Address;
		_web3 = new Web3(account, $"https://eth-sepolia.g.alchemy.com/v2/{alchemyApiKey}");
	}

	[HttpPost]
	public async Task<IActionResult> Vote([FromBody] VoteRequest request)
	{
		if (string.IsNullOrEmpty(request.CandidateName))
			return BadRequest("Invalid vote");

		var contractAbi = GetContractAbi(@"D:\Personal stuff\UPT\Anul 4\Licenta\BlockchainE-VotingSystem\BlockchainService\artifacts\contracts\Voting.sol\Voting.json");
		var contract = _web3.Eth.GetContract(contractAbi, _contractAddress);

		var getVotingStatusFunction = contract.GetFunction("getVotingStatus");
		bool votingOpen = await getVotingStatusFunction.CallAsync<bool>();

		//if (!votingOpen)
		//	return BadRequest("Voting is finished");

		var voteFunction = contract.GetFunction("addCandidate");
		var transactionHash = await voteFunction.SendTransactionAsync(_accountAddress, new HexBigInteger(3000000), new HexBigInteger(0), request.CandidateName);

		return Ok(new { Message = "Vote recorded successfully", TransactionHash = transactionHash });
	}

	static string GetContractAbi(string filePath)
	{
		try
		{
			// Read JSON file
			string jsonString = System.IO.File.ReadAllText(filePath);

			// Parse JSON
			using JsonDocument doc = JsonDocument.Parse(jsonString);

			// Extract ABI array
			JsonElement root = doc.RootElement;
			if (root.TryGetProperty("abi", out JsonElement abiElement))
			{
				return abiElement.ToString(); // Convert ABI to string
			}

			return "ABI not found in JSON file.";
		}
		catch (Exception ex)
		{
			return $"Error reading ABI: {ex.Message}";
		}
	}
}

public class VoteRequest
{
	public string? CandidateName { get; set; }
}



