using Azure.AI.OpenAI;
using ContosoChatAPI.Data;
using Newtonsoft.Json;
using static ContosoChatAPI.Data.CustomerData;
<<<<<<< HEAD
using ContosoChatAPI.Evaluations;
=======
>>>>>>> af1c39a (contoso chat logic added)


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

        public ChatService() {
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
<<<<<<< HEAD
                { "documentation", context },
                { "question", question },
=======
                { "context", context },
                { "embedding", embedding },
>>>>>>> af1c39a (contoso chat logic added)
                { "chatHistory", chatHistory }
            };
            // load chat.json file as new dictionary<string, string>
            //var jsonInputs = System.IO.File.ReadAllText("chat.json");
            // convert json to dictionary<string, string>
            //var inputs = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonInputs);

            var prompty = new Prompty.Core.Prompty();
            prompty.Inputs = inputs;

            Console.WriteLine("Getting result...");
            prompty = await prompty.Execute("chat.prompty", prompty);
<<<<<<< HEAD
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
=======

            // not implemented yet
            //var score = new Dictionary<string, double>();
            //score["groundedness"] = EvaluateGroundedness(question, context, result);
            //score["coherence"] = EvaluateCoherence(question, context, result);
            //score["relevance"] = EvaluateRelevance(question, context, result);
            //score["fluency"] = EvaluateFluency(question, context, result);

            //Console.WriteLine($"Result: {result}");
            //Console.WriteLine($"Score: {string.Join(", ", score)}");

            return prompty.ChatResponseMessage.Content;
>>>>>>> af1c39a (contoso chat logic added)
        }
    }
}
