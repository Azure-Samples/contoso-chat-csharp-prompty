using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;
using Azure.AI.OpenAI;

namespace ContosoChatAPI.Evaluations
{
    public class Evaluation
    {
       private readonly ILogger<Evaluation> logger;
        private readonly OpenAIClient openaiClient;
        private readonly string deploymentName;

        public Evaluation(ILogger<Evaluation> logger, OpenAIClient openaiClient, IConfiguration config)
        {
            this.logger = logger;
            this.openaiClient = openaiClient;
            this.deploymentName = config["OpenAi:deployment"];
        }

        // Run a batch coherence evaluation
        public async Task<List<string>> Batch(string file, string prompty)
        {
            if(!File.Exists(file))
            {
                file =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.jsonl");
            }

            var results = new List<string>();
            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var data = JObject.Parse(line);
                var result = await Evaluate(data["question"].ToString(), data["context"], data["answer"].ToString(), prompty);
                results.Add(result);
            }

            return results;
        }

        // Run a single coherence evaluation
        public async Task<string> Evaluate(string question, object context, string answer, string prompty)
        {
            var kernel = Kernel.CreateBuilder()
                                .AddAzureOpenAIChatCompletion(deploymentName, openaiClient)
                                .Build();

            var cwd = Directory.GetCurrentDirectory();
            var chatPromptyPath = Path.Combine(cwd, prompty);

#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var kernelFunction = kernel.CreateFunctionFromPrompty(chatPromptyPath);
#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            logger.LogInformation("Getting result...");
            var arguments = new KernelArguments(){
                { "answer", answer },
                { "context", context },
                { "question", question }
            };

            var kernalResult = await kernelFunction.InvokeAsync(kernel, arguments);
            //get string result

            // Create score dict with results
            var message = kernalResult.ToString();

            return message;
        }
    }
}
