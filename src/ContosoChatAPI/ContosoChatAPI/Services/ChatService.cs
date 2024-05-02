using ContosoChatAPI.Data;
using Newtonsoft.Json;
using ContosoChatAPI.Evaluations;
using Microsoft.SemanticKernel;
using System.Configuration;
using Azure.AI.OpenAI;


namespace ContosoChatAPI.Services
{
    public class ChatService
    {
        private readonly CustomerData _customerData;
        private readonly AISearchData _aiSearch;
        private readonly EmbeddingData _embeddingData;
        private readonly Evaluation _evaluation;
        private readonly ILogger<ChatService> _logger;
        private readonly OpenAIClient _openaiClient;
        private readonly string _deploymentName;

        public ChatService(CustomerData customerData, AISearchData aiSearch, EmbeddingData embeddingData, Evaluation evaluation, ILogger<ChatService> logger, OpenAIClient openaiClient, IConfiguration config)
        {
            _customerData = customerData;
            _aiSearch = aiSearch;
            _embeddingData = embeddingData;
            _evaluation = evaluation;
            _logger = logger;
            _openaiClient = openaiClient;
            _deploymentName = config["OpenAi:deployment"];
        }

        public async Task<string> GetResponseAsync(string customerId, string question, List<string> chatHistory)
        {
            _logger.LogInformation($"Inputs: CustomerId = {customerId}, Question = {question}");

            var customer = await _customerData.GetCustomerAsync(customerId);
            var embedding = await _embeddingData.GetEmbedding(question);
            var context = await _aiSearch.RetrieveDocumentationAsync(question, embedding);

            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(_deploymentName, _openaiClient)
                .Build();

            var cwd = Directory.GetCurrentDirectory();
            var chatPromptyPath = Path.Combine(cwd, "chat.prompty");

#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var kernelFunction = kernel.CreateFunctionFromPrompty(chatPromptyPath);
#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            _logger.LogInformation("Getting result...");
            var arguments = new KernelArguments(){
                { "customer", customer },
                { "documentation", context },
                { "question", question },
                { "chatHistory", chatHistory }
            };

            var kernalResult = await kernelFunction.InvokeAsync(kernel, arguments);
            //get string result

            // Create score dict with results
            var score = new Dictionary<string, string>();
            var message = kernalResult.ToString();

            score["groundedness"] = await _evaluation.Evaluate(question, context, message, "./Evaluations/groundedness.prompty");
            score["coherence"] = await _evaluation.Evaluate(question, context, message, "./Evaluations/coherence.prompty");
            score["relevance"] = await _evaluation.Evaluate(question, context, message, "./Evaluations/relevance.prompty");
            score["fluency"] = await _evaluation.Evaluate(question, context, message, "./Evaluations/fluency.prompty");

            _logger.LogInformation($"Result: {kernalResult}");
            _logger.LogInformation($"Score: {string.Join(", ", score)}");
            // add score to result

            var result = JsonConvert.SerializeObject(new { message, score });

            return result;
        }
    }
}