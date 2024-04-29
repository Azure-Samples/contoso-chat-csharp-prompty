using ContosoChatAPI.Data;
using Newtonsoft.Json;
using static ContosoChatAPI.Data.CustomerData;
using ContosoChatAPI.Evaluations;
using Microsoft.SemanticKernel;
using Azure.AI.OpenAI;
using Azure;


namespace ContosoChatAPI.Services
{
    public class ChatService
    {
        private const string _deploymentName = "gpt-35-turbo";
        private readonly IConfiguration _consmos;
        private readonly string _cosmosEndpoint;
        private readonly string _cosmosKey;

        private readonly IConfiguration _prompty;
        private readonly string _oaiEndpoint;
        private readonly string _oaiKey;

        private readonly IConfiguration _aiSearch;
        private readonly string _aiSearchEndpoint;
        private readonly string _aiSearchKey;

        //public static IKernelBuilder builder;

        private readonly OpenAIClient _client;

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

            _client = new OpenAIClient(
                    endpoint: new Uri(_oaiEndpoint),
                    keyCredential: new AzureKeyCredential(_oaiKey)
);
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
            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(_deploymentName, _client)
                .Build();

            var cwd = Directory.GetCurrentDirectory();
            var chatPromptyPath = Path.Combine(cwd, "chat.prompty");

            var kernelFunction = kernel.CreateFunctionFromPrompty(chatPromptyPath);

            Console.WriteLine("Getting result...");
            var arguments = new KernelArguments(){
                { "customer", customer },
                { "documentation", context },
                { "question", question },
                { "chatHistory", chatHistory }
            };

            var kernalResult = kernelFunction.InvokeAsync(kernel, arguments).Result;
            //get string result

            // Create score dict with results
            var score = new Dictionary<string, string>();
            var message = kernalResult.ToString();

            score["groundedness"] = await Evaluation.Evaluate(question, context, message, "./Evaluations/groundedness.prompty", _deploymentName, _client);
            score["coherence"] = await Evaluation.Evaluate(question, context, message, "./Evaluations/coherence.prompty", _deploymentName, _client);
            score["relevance"] = await Evaluation.Evaluate(question, context, message, "./Evaluations/relevance.prompty", _deploymentName, _client);
            score["fluency"] = await Evaluation.Evaluate(question, context, message, "./Evaluations/fluency.prompty", _deploymentName, _client);

            Console.WriteLine($"Result: {kernalResult}");
            Console.WriteLine($"Score: {string.Join(", ", score)}");
            // add score to result
            
            var result = JsonConvert.SerializeObject(new { message, score });

            return result;
        }
    }
}