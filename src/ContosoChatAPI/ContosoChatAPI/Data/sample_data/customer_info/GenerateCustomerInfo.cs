using System.Text;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace ContosoChatAPI.Data
{
    public class GenerateCustomerInfo
    {
        private ILogger<GenerateCustomerInfo> _logger;
        private IConfiguration _config;
        private readonly CosmosClient _cosmosClient;
        private readonly string _indexName;
        private readonly string _databaseName;
        private readonly string _containerName;

        public GenerateCustomerInfo(ILogger<GenerateCustomerInfo> logger, IConfiguration config, CosmosClient cosmosClient)
        {
            _logger = logger;
            _config = config;
            _cosmosClient = cosmosClient;
            _indexName = config["AzureAISearch:index_name"];
            _databaseName = config["CosmosDb:databaseName"];
            _containerName = config["CosmosDb:containerName"];
        }

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
                    _logger.LogInformation($"Creating CosmosDB container {_containerName} in database {_databaseName}...");

                    var jsonFiles = Directory.GetFiles("./Data/sample_data/customer_info", "*.json");
                    foreach (string file in jsonFiles)
                    {
                        var content = await File.ReadAllTextAsync(file);
                        var customer = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                        await container.Container.CreateItemStreamAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)), new PartitionKey(customer["id"].ToString()));

                        _logger.LogInformation($"Upserted item with id {customer["id"]}");
                    }
                }
                else
                {
                    _logger.LogInformation("CosmosDB container already populated, nothing to do.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Cosmos: " + ex.Message);
            }
        }

    }
}