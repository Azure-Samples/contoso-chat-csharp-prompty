using ContosoChatAPI.Data;
using Newtonsoft.Json;
using static ContosoChatAPI.Data.CustomerData;
using ContosoChatAPI.Evaluations;
using Prompty.Core;


namespace ContosoChatAPI.Services
{
    public class ChatService
    {
        private readonly IConfiguration _consmos;
        private readonly string _cosmosEndpoint;
        private readonly string _cosmosKey;

        private readonly IConfiguration _prompty;
        private readonly string _oaiEndpoint;
        private readonly string _oaiKey;

        private readonly IConfiguration _aiSearch;
        private readonly string _aiSearchEndpoint;
        private readonly string _aiSearchKey;

        public ChatService()
        {
            // get endpoint and key from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            _consmos = config.GetSection("CosmosDB");
            _prompty = config.GetSection("prompty");
            _aiSearch = config.GetSection("AzureAISearch");

            _cosmosEndpoint = _consmos["Endpoint"];
            _cosmosKey = _consmos["Key"];

            _oaiEndpoint = _prompty["azure_endpoint"];
            _oaiKey = _prompty["api_key"];

            _aiSearchEndpoint = _aiSearch["Endpoint"];
            _aiSearchKey = _aiSearch["Key"];
        }
        public async Task<string> GetResponseAsync(string customerId, string question, List<string> chatHistory)
        {

            Console.WriteLine($"Inputs: CustomerId = {customerId}, Question = {question}");

            // get customer data
            var customerData = new CustomerData(_cosmosEndpoint, _cosmosKey);
            var customer = await customerData.GetCustomerAsync(customerId);
            // get question embedding

            var embeddingData = new EmbeddingData(_oaiEndpoint, _oaiKey);
            var embedding = embeddingData.GetEmbedding(question);

            var aiSearch = new AISearchData(_aiSearchEndpoint, _aiSearchKey, "contoso-products");
            var context = await aiSearch.RetrieveDocumentationAsync(question, embedding);

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

            Console.WriteLine("Getting result...");
            prompty = await prompty.Execute(prompty);
            var result = prompty.ChatResponseMessage.Content;

            // Create score dict with results
            var score = new Dictionary<string, string>();

            score["groundedness"] = await Evaluation.Evaluate(question, context, result, "./Evaluations/groundedness.prompty");
            score["coherence"] = await Evaluation.Evaluate(question, context, result, "./Evaluations/coherence.prompty");
            score["relevance"] = await Evaluation.Evaluate(question, context, result, "./Evaluations/relevance.prompty");
            score["fluency"] = await Evaluation.Evaluate(question, context, result, "./Evaluations/fluency.prompty");

            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Score: {string.Join(", ", score)}");
            // add score to result
            result = JsonConvert.SerializeObject(new { result, score });

            return result;
        }
    }
}