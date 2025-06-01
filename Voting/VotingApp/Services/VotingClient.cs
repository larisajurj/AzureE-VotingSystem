using Azure.Core;
using System;
using VotingApp.Models;
using VotingApp.Services.Abstractions;

namespace VotingApp.Services;

public class VotingClient : IVotingClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly string _clientName;

    public VotingClient(IHttpClientFactory clientFactory)
    {
        this._clientFactory = clientFactory;
        this._clientName = "BoothClient";
    }

    public async Task<bool> SendVoteAsync(Ballot vote, CancellationToken cancellationToken = default)
    {
        if (vote == null)
        {
            Console.WriteLine("Error: Vote object cannot be null.");
            throw new ArgumentNullException(nameof(vote));
        }

        try
        {
            var httpClient = _clientFactory.CreateClient(_clientName);

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(vote);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("api/SendVote", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // The Azure Function returns "Successfully registered" as OkObjectResult content
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Vote sent successfully. Server response: {responseContent}");
                return true;
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Failed to send vote. Status: {response.StatusCode}. Details: {errorContent}");
                return false;
            }
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Console.WriteLine("Sending vote was canceled.");
            return false;
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"An unexpected error occurred while sending vote: {ex.Message}");
            return false;
        }
    }
}
