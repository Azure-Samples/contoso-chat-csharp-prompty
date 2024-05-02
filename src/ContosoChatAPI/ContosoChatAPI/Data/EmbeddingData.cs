using Azure.AI.OpenAI;

namespace ContosoChatAPI.Data
{
    public class EmbeddingData
    {
        private readonly OpenAIClient _openAIClient;
        private readonly IConfiguration config;
        private readonly ILogger<EmbeddingData> logger;

        public EmbeddingData(IConfiguration config, ILogger<EmbeddingData> logger, OpenAIClient openAIClient)
        {
            _openAIClient = openAIClient;
            this.config = config;
            this.logger = logger;
        }
        public async Task<Embeddings> GetEmbedding(string question)
        {
            try
            {
                EmbeddingsOptions embeddingOptions = new()
                {
                    DeploymentName = config["OpenAi:embedding_deployment"],
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

