using System.Text;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace ContosoChatAPI.Data;

public sealed class GenerateCustomerInfo(ILogger<GenerateCustomerInfo> logger, IConfiguration config, CosmosClient cosmosClient)
{
    private readonly ILogger<GenerateCustomerInfo> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly string _databaseName = config["CosmosDb:databaseName"]!;
    private readonly string _containerName = config["CosmosDb:containerName"]!;

    public async Task PopulateCosmosAsync()
    {
        try
        {
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
            var container = await database.Database.CreateContainerIfNotExistsAsync(_containerName, "/id");

            var numDocs = 0;

            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
            using (var iterator = container.Container.GetItemQueryIterator<int>(query))
            {
                var result = await iterator.ReadNextAsync();
                numDocs = result.FirstOrDefault();
            }

            if (numDocs == 0)
            {
                _logger.LogInformation("Creating CosmosDB container {ContainerName} in database {DatabaseName}...", _containerName, _databaseName);

                var jsonFiles = Directory.GetFiles("./Data/sample_data/customer_info", "*.json");
                foreach (string file in jsonFiles)
                {
                    var content = await File.ReadAllTextAsync(file);
                    var customer = JsonSerializer.Deserialize<Dictionary<string, object>>(content)!;

                    await container.Container.CreateItemStreamAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)), new PartitionKey(customer["id"].ToString()));

                    _logger.LogInformation("Upserted item with id {CustomerID}", customer["id"]);
                }
            }
            else
            {
                _logger.LogInformation("CosmosDB container already populated, nothing to do.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating Cosmos");
        }
    }
}