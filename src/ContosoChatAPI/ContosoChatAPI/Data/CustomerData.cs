using Microsoft.Azure.Cosmos;

namespace ContosoChatAPI.Data;

public sealed class CustomerData(CosmosClient cosmosClient, ILogger<CustomerData> logger, IConfiguration config)
{
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly ILogger<CustomerData> logger = logger;
    private readonly string _databaseName = config["CosmosDb:databaseName"]!;
    private readonly string _containerName = config["CosmosDb:containerName"]!;

    public async Task<Dictionary<string, object>> GetCustomerAsync(string customerId)
    {
        try
        {
            var container = _cosmosClient.GetContainer(_databaseName, _containerName);

            var response = await container.ReadItemAsync<Dictionary<string, object>>(customerId, new PartitionKey(customerId));
            var customer = response.Resource;

            // Limit orders to the first 2 items
            if (customer.TryGetValue("orders", out object? orders) && orders is List<object> list)
            {
                customer["orders"] = list.Take(2).ToList();
            }

            return customer;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogError("Customer with ID {CustomerID} not found.", customerId);
            throw;
        }
    }
}
