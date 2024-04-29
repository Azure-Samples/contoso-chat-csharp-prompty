using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;
using Azure.AI.OpenAI;

namespace ContosoChatAPI.Evaluations
{
    public static class Evaluation
    {
        // Run a batch coherence evaluation
        public static async Task<List<string>> Batch(string file, string prompty, string deploymentName, OpenAIClient client)
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
                var result = await Evaluate(data["question"].ToString(), data["context"], data["answer"].ToString(), prompty, deploymentName, client);
                results.Add(result);
            }

            return results;
        }

        // Run a single coherence evaluation
        public static async Task<string> Evaluate(string question, object context, string answer, string prompty, string deploymentName, OpenAIClient client)
        {
            var kernel = Kernel.CreateBuilder()
                                .AddAzureOpenAIChatCompletion(deploymentName, client)
                                .Build();

            var cwd = Directory.GetCurrentDirectory();
            var chatPromptyPath = Path.Combine(cwd, prompty);

            var kernelFunction = kernel.CreateFunctionFromPrompty(chatPromptyPath);

            Console.WriteLine("Getting result...");
            var arguments = new KernelArguments(){
                { "answer", answer },
                { "context", context },
                { "question", question }
            };

            var kernalResult = kernelFunction.InvokeAsync(kernel, arguments).Result;
            //get string result

            // Create score dict with results
            var message = kernalResult.ToString();

            return message;
        }
    }
}
