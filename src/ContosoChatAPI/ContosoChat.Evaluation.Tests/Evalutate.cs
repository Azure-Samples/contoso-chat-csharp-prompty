using Microsoft.SemanticKernel;
using System.Text.Json;
using ContosoChatAPI.Services;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Xunit;
using ContosoChatAPI.Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;


namespace ContosoChat.Evaluation.Tests
{
    public class Evalutate
    {
        //create chatService and serviceProvider
        private readonly ChatService _chatService;
        private readonly ServiceProvider _serviceProvider;
        
        public Evalutate()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configurationBuilder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(serviceProvider => new CosmosClient(config["CosmosDb:Endpoint"], new DefaultAzureCredential()));
            serviceCollection.AddSingleton(new OpenAIClient(new Uri(config["OpenAi:endpoint"]!), new DefaultAzureCredential()));
            serviceCollection.AddKernel();
            serviceCollection.AddAzureOpenAIChatCompletion(config["OpenAi:deployment"]!);
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            serviceCollection.AddAzureOpenAITextEmbeddingGeneration(config["OpenAi:embedding_deployment"]!);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            serviceCollection.AddSingleton(new SearchClient(new Uri(config["AzureAISearch:Endpoint"]!), config["AzureAISearch:index_name"], new DefaultAzureCredential()));
            serviceCollection.AddSingleton(new SearchIndexClient(new Uri(config["AzureAISearch:Endpoint"]!), new DefaultAzureCredential()));
            serviceCollection.AddScoped<CustomerData>();
            serviceCollection.AddSingleton<GenerateCustomerInfo>();
            serviceCollection.AddSingleton<GenerateProductInfo>();
            serviceCollection.AddScoped<AISearchData>();
            serviceCollection.AddScoped<ChatService>();
            serviceCollection.AddLogging();
            // add IConfiguration to serviceCollection
            serviceCollection.AddSingleton<IConfiguration>(config);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _chatService = _serviceProvider.GetRequiredService<ChatService>();
        }

        //Test EvaluationResult
        [Theory]
        [InlineData("Tell me about your jackets?", "1")]
        public async void EvaluationResult(string question, string customerId)
        {

            // GetResponse from chat service
            var result = await _chatService.GetResponseAsync(customerId, question);
            // parse result string varibales of context and answer
            var response = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(result);
            var answer = response?["answer"];
            var context = response?["context"];
            

            //GetEvaluation from chat service
            var score = await _chatService.GetEvaluationAsync(question, context, answer);

            var coherence = int.Parse(score["coherence"]);
            var groundedness = int.Parse(score["groundedness"]);
            var relevance = int.Parse(score["relevance"]);
            var fluency = int.Parse(score["fluency"]);



            Assert.Multiple(
                () => Assert.True(coherence >= 3, $"Coherence of {question} - score {coherence}, expecting min 3."),
                    () => Assert.True(groundedness >= 3, $"Groundedness of {question} - score {groundedness}, expecting min 3."),
                    () => Assert.True(relevance >= 2, $"Relevance of {question} - score {relevance}, expecting min 2."),
                    () => Assert.True(fluency >= 2, $"Fluency of {question} - score {fluency}, expecting min 2.")
                );
        }

    }
}