using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using Azure.Identity;

namespace ContosoChatAPI.Data
{
    public class CustomerData
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<CustomerData> logger;
        private readonly string _databaseName = "contoso-outdoor";
        private readonly string _containerName = "customers";

        public CustomerData(CosmosClient cosmosClient, ILogger<CustomerData> logger)
        {
            _cosmosClient = cosmosClient;
            this.logger = logger;
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
