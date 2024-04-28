using Azure.AI.OpenAI;
using Azure;
using System.Text.Json;
using Prompty.Core.Types;
using System.Collections;
using Azure.Identity;

namespace Prompty.Core.Executors
{
    public class AzureOpenAIExecutor
    {
        private readonly OpenAIClient client;
        private readonly string api;
        private readonly string deployment;
        private readonly Dictionary<string, dynamic> parameters;
        private readonly ChatCompletionsOptions chatCompletionsOptions;
        private readonly CompletionsOptions completionsOptions;
        private readonly ImageGenerationOptions imageGenerationOptions;
        private readonly EmbeddingsOptions embeddingsOptions;

        public AzureOpenAIExecutor(Prompty prompty)
        {
            if (string.IsNullOrEmpty(prompty.Model.ApiKey))
            {
                client = new OpenAIClient(endpoint: new Uri(prompty.Model.AzureEndpoint), new DefaultAzureCredential());
            }
            else
            {
                client = new OpenAIClient(endpoint: new Uri(prompty.Model.AzureEndpoint), keyCredential: new AzureKeyCredential(prompty.Model.ApiKey));
            }

            api = prompty.modelApiType.ToString();
            parameters = prompty.Parameters;

            chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = prompty.Model.AzureDeployment
            };
            completionsOptions = new CompletionsOptions()
            {
                DeploymentName = prompty.Model.AzureDeployment
            };
            imageGenerationOptions = new ImageGenerationOptions()
            {
                DeploymentName = prompty.Model.AzureDeployment
            };
            embeddingsOptions = new EmbeddingsOptions()
            {
                DeploymentName = prompty.Model.AzureDeployment
            };

        }

        public async Task<Prompty> GetChatCompletiom(Prompty prompty)
        {

            if (api == ApiType.Chat.ToString())
            {
                try
                {


                    for (int i = 0; i < prompty.Messages.Count; i++)
                    {
                        //parse role sting to enum value
                        var roleEnum = Enum.Parse<RoleType>(prompty.Messages[i]["role"]);

                        switch (roleEnum)
                        {
                            case RoleType.user:
                                var userMessage = new ChatRequestUserMessage(prompty.Messages[i]["content"]);
                                chatCompletionsOptions.Messages.Add(userMessage);
                                break;
                            case RoleType.system:
                                var systemMessage = new ChatRequestSystemMessage(prompty.Messages[i]["content"]);
                                chatCompletionsOptions.Messages.Add(systemMessage);
                                break;
                            case RoleType.assistant:
                                var assistantMessage = new ChatRequestAssistantMessage(prompty.Messages[i]["content"]);
                                chatCompletionsOptions.Messages.Add(assistantMessage);
                                break;
                            case RoleType.function:
                                //TODO: Fix parsing for Function role
                                var functionMessage = new ChatRequestFunctionMessage("name", prompty.Messages[i]["content"]);
                                chatCompletionsOptions.Messages.Add(functionMessage);
                                break;
                        }

                    }
                    var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                    prompty.ChatResponseMessage = response.Value.Choices[0].Message;

                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                }
            }
            else if (api == ApiType.Completion.ToString())
            {
                try
                {
                    var response = await client.GetCompletionsAsync(completionsOptions);
                    prompty.CompletionResponseMessage = response.Value;

                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                }
            }
            else if (api == ApiType.Embedding.ToString())
            {
                try
                {
                    var response = await client.GetEmbeddingsAsync(embeddingsOptions);
                    prompty.EmbeddingResponseMessage = response.Value;

                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                }
            }
            else if (api == ApiType.Image.ToString())
            {
                try
                {
                    var response = await client.GetImageGenerationsAsync(imageGenerationOptions);
                    prompty.ImageResponseMessage = response.Value;

                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                }
            }


            return prompty;
        }

    }

}
