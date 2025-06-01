using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
using System.Security.Cryptography.Xml;
using PollingStationApp.Components;
using PollingStationApp.Data.Helpers.Abstractions;
using PollingStationApp.Models;
using PollingStationApp.Services.Abstractions;

namespace PollingStationApp.Services;

public class PollingStationClient : IPollingStationClient
{
    private readonly IHttpClientFactory clientFactory;
    //private readonly ITokenProvider tokenProvider;
    private readonly string clientName;
    protected ClaimsPrincipal? user;
    private ITokenProvider tokenProvider;

    public PollingStationClient(ITokenProvider _tokenProvider, IHttpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory;
        this.clientName = "PollingStationClient";
        this.tokenProvider = _tokenProvider;

    }
    public void SetUser(ClaimsPrincipal user)
    {
        this.user = user;
    }

    public async Task<PollingStation?> GetStationById(string pollingStationId)
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {
            try
            {
                var token = await tokenProvider.GetAccessTokenAsync(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync($"/api/PollingStation/{pollingStationId}");
                var content = await result.Content.ReadFromJsonAsync<PollingStation>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        else return null;
    }

    public async Task<PollingStation?> GetStationByUserId()
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {
            try
            {
                var committeeMemberId = user.FindFirst("oid")?.Value;
                var token = await tokenProvider.GetAccessTokenAsync(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync($"/api/PollingStation/ByUserId/{committeeMemberId}");
                var content = await result.Content.ReadFromJsonAsync<PollingStation>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        else return null;
    }

    public async Task<CommitteeMember?> GetCommitteeMember()
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {
            try
            {
                var committeeMemberId = user.FindFirst("oid")?.Value;
                var token = await tokenProvider.GetAccessTokenAsync(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync($"/api/CommitteeMember/{committeeMemberId}");
                var content = await result.Content.ReadFromJsonAsync<CommitteeMember>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        else return null;
    }

    public async Task<Booth?> GetBoothById(string pollingStationId, int boothId)
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {
            try
            {
                var token = await tokenProvider.GetAccessTokenAsync(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync($"/api/PollingStation/{pollingStationId}/booth/{boothId}");
                var content = await result.Content.ReadFromJsonAsync<Booth>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        else return null;
    }

    public async Task SaveSignature(Guid recordId, string signature)
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {

            var token = await tokenProvider.GetAccessTokenAsync(user);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(signature); 

            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");


            var result = await client.PostAsync($"/api/VotingRecord/{recordId}/signature", content);
            result.EnsureSuccessStatusCode();
        }
        else
        {
            throw new UnauthorizedAccessException();
        }
    }

    public async Task<List<VotingRecord>?> GetRecordsByStatus(string assignedMemberId, string status)
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {
            try
            {
                var token = await tokenProvider.GetAccessTokenAsync(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync($"/api/VotingRecord/byAssignedMemberId/{assignedMemberId}/status/{status}");
                var content = await result.Content.ReadFromJsonAsync<List<VotingRecord>>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        else return null;
    }

    public async Task<List<Booth>> GetBooths(string pollingStationId)
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {
            try
            {
                var token = await tokenProvider.GetAccessTokenAsync(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync($"/api/PollingStation/{pollingStationId}/booths");
                var content = await result.Content.ReadFromJsonAsync<List<Booth>>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        else return null;
    }

    public async Task ChangeStatusOfRecord(Guid voterId, string status)
    {
        var client = this.clientFactory.CreateClient(this.clientName);

        if (user != null)
        {

            var token = await tokenProvider.GetAccessTokenAsync(user);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(status);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");


            var result = await client.PatchAsync($"/api/VotingRecord/byVoter/{voterId}/status", content);
            result.EnsureSuccessStatusCode();
        }
        else
        {
            throw new UnauthorizedAccessException();
        }
    }
}
