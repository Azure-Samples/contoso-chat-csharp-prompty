using ContosoChatAPI.Data;
using Newtonsoft.Json;
using static ContosoChatAPI.Data.CustomerData;
using ContosoChatAPI.Evaluations;
using Prompty.Core;
using Microsoft.Azure.Cosmos;


namespace ContosoChatAPI.Services
{
    public class ChatService
    {
        private readonly CustomerData _customerData;
        private readonly AISearchData _aiSearch;
        private readonly EmbeddingData _embeddingData;
        private readonly Evaluation _evaluation;
        private readonly ILogger<ChatService> logger;

        public ChatService(CustomerData customerData, AISearchData aiSearch, EmbeddingData embeddingData, Evaluation evaluation, ILogger<ChatService> logger)
        {
            _customerData = customerData;
            _aiSearch = aiSearch;
            _embeddingData = embeddingData;
            _evaluation = evaluation;
            this.logger = logger;
        }

        public async Task<string> GetResponseAsync(string customerId, string question, List<string> chatHistory)
        {
            logger.LogInformation($"Inputs: CustomerId = {customerId}, Question = {question}");

            var customer = await _customerData.GetCustomerAsync(customerId);
            var embedding = await _embeddingData.GetEmbedding(question);
            var context = await _aiSearch.RetrieveDocumentationAsync(question, embedding);

            var inputs = new Dictionary<string, dynamic>
            {
                { "customer", customer },
                { "documentation", context },
                { "question", question },
                { "chatHistory", chatHistory }
            };

            var prompty = new Prompty.Core.Prompty();
            // load prompty file
            prompty.Load("chat.prompty", prompty);

            // set overides
            prompty.Inputs = inputs;

            logger.LogInformation("Getting result...");

            prompty = await prompty.Execute(prompty);
            var result = prompty.ChatResponseMessage.Content;

            // Create score dict with results
            var score = new Dictionary<string, string>
            {
                ["groundedness"] = await _evaluation.Evaluate(question, context, result, "./Evaluations/groundedness.prompty"),
                ["coherence"] = await _evaluation.Evaluate(question, context, result, "./Evaluations/coherence.prompty"),
                ["relevance"] = await _evaluation.Evaluate(question, context, result, "./Evaluations/relevance.prompty"),
                ["fluency"] = await _evaluation.Evaluate(question, context, result, "./Evaluations/fluency.prompty")
            };

            logger.LogInformation($"Result: {result}");
            logger.LogInformation($"Score: {string.Join(", ", score)}");
            // add score to result
            result = JsonConvert.SerializeObject(new { result, score });

            return result;
        }
    }
}