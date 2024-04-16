using Azure.AI.OpenAI;
using Azure;

namespace ContosoChatAPI.Data
{
    public class EmbeddingData
    {

        private readonly AzureKeyCredential _credentials;
        private readonly OpenAIClient _openAIClient; 

        public EmbeddingData(string oaiEndpoint, string oaiKey)
        {
            _credentials = new(oaiKey);
            _openAIClient = new(new Uri(oaiEndpoint), _credentials);

        }
        public Embeddings GetEmbedding(string question)
        {
            try
            {
                EmbeddingsOptions embeddingOptions = new()
                {
                    DeploymentName = "text-embedding-ada-002",
                    Input = { question },
                };

                var embeddings = _openAIClient.GetEmbeddings(embeddingOptions);
                return embeddings.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
    }
}

