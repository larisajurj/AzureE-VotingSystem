namespace PollingStationAPI.Data.Repository;

using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository.Abstractions;
using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Threading.Tasks;

public class PollingStationRepository : IRepository<PollingStation, int>
{
    private readonly Container _container;

    // Constants for database and container names (as in your Terraform)
    private const string DatabaseName = "VotingDatabase";
    private const string ContainerName = "PollingStation";

    public PollingStationRepository(CosmosClient cosmosClient)
    {
        if (cosmosClient == null)
            throw new ArgumentNullException(nameof(cosmosClient));

        _container = cosmosClient.GetContainer(DatabaseName, ContainerName);
    }

    public async Task Add(PollingStation entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            // The entity.Id (string) is used as the partition key value
            await _container.CreateItemAsync(entity, new PartitionKey(entity.Id));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Handle case where item with the same ID already exists
            // You might want to log this, throw a custom exception, or return a specific result
            Console.WriteLine($"Item with ID {entity.Id} already exists. Exception: {ex.Message}");
            // Depending on requirements, you might re-throw or handle differently
            throw;
        }
        catch (CosmosException ex)
        {
            // Handle other potential Cosmos DB errors
            Console.WriteLine($"Cosmos DB Exception during Add: {ex.Message}, Status Code: {ex.StatusCode}");
            throw; // Re-throw to allow higher-level error handling
        }
    }

    public async Task<PollingStation?> GetById(int entityId)
    {
        string cosmosId = entityId.ToString(); // Convert int to string for Cosmos DB ID

        try
        {
            // The cosmosId (string) is used for both item ID and partition key value
            ItemResponse<PollingStation> response = await _container.ReadItemAsync<PollingStation>(cosmosId, new PartitionKey(cosmosId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null; // Item not found
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Cosmos DB Exception during GetById for ID {cosmosId}: {ex.Message}, Status Code: {ex.StatusCode}");
            throw;
        }
    }

    public async Task<PollingStation?> Update(PollingStation entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            // entity.Id (string) is used for item ID and partition key value
            ItemResponse<PollingStation> response = await _container.ReplaceItemAsync(entity, entity.Id.ToString(), new PartitionKey(entity.Id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Item to update was not found, could return null or throw
            Console.WriteLine($"Item with ID {entity.Id} not found for update. Exception: {ex.Message}");
            return null;
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Cosmos DB Exception during Update for ID {entity.Id}: {ex.Message}, Status Code: {ex.StatusCode}");
            throw;
        }
    }

    public async Task<bool> Delete(int entityId)
    {
        string cosmosId = entityId.ToString(); // Convert int to string for Cosmos DB ID

        try
        {
            // cosmosId (string) is used for item ID and partition key value
            await _container.DeleteItemAsync<PollingStation>(cosmosId, new PartitionKey(cosmosId));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false; // Item not found, so delete operation "succeeded" in that it's gone
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Cosmos DB Exception during Delete for ID {cosmosId}: {ex.Message}, Status Code: {ex.StatusCode}");
            // Depending on how strict you want to be, you might return false or re-throw
            return false; // Or throw;
        }
    }
}
