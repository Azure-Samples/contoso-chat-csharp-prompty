using Microsoft.Azure.Cosmos;

namespace ContosoChatAPI.Data
{
    public class CustomerData
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<CustomerData> logger;
        private readonly string _databaseName;
        private readonly string _containerName;

        public CustomerData(CosmosClient cosmosClient, ILogger<CustomerData> logger, IConfiguration config)
        {
            _cosmosClient = cosmosClient;
            this.logger = logger;
            _databaseName = config["CosmosDb:databaseName"];
            _containerName = config["CosmosDb:containerName"];
        }

        public async Task<Dictionary<string, object>> GetCustomerAsync(string customerId)
        {
            var container = _cosmosClient.GetContainer(_databaseName, _containerName);
            var partitionKey = new PartitionKey(customerId);

            try
            {
                var response = await container.ReadItemAsync<Dictionary<string, object>>(customerId, partitionKey);
                var customer = response.Resource;

                // Limit orders to the first 2 items
                if (customer.ContainsKey("orders") && customer["orders"] is List<object> orders)
                {
                    customer["orders"] = orders.Take(2).ToList();
                }

                return customer;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogError($"Customer with ID {customerId} not found.");
                throw;
            }
        }

    }
}
