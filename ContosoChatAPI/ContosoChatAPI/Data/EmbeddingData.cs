using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;

namespace ContosoChatAPI.Data
{
    public class EmbeddingData
    {

        private readonly AzureKeyCredential _credentials;
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<EmbeddingData> logger;

        public EmbeddingData(IConfiguration config, ILogger<EmbeddingData> logger)
        {
            var promptyConfig = config.GetSection("prompty");

            _openAIClient = new(new Uri(promptyConfig["azure_endpoint"]), new DefaultAzureCredential());
            this.logger = logger;
        }
        public async Task<Embeddings> GetEmbedding(string question)
        {
            try
            {
                EmbeddingsOptions embeddingOptions = new()
                {
                    DeploymentName = "text-embedding-ada-002",
                    Input = { question },
                };

                var embeddings = await _openAIClient.GetEmbeddingsAsync(embeddingOptions);
                return embeddings.Value;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}

