using Prompty.Core.Parsers;
using Prompty.Core.Renderers;
using Prompty.Core.Executors;
using YamlDotNet.Serialization;
using Prompty.Core.Types;
using Azure.AI.OpenAI;

namespace Prompty.Core
{

    public class Prompty()
    {
        // PromptyModelConfig model, string prompt, bool isFromSettings = true
        // TODO: validate  the prompty attributes needed, what did I miss that should be included?
        [YamlMember(Alias = "name")]
        public string? Name;

        [YamlMember(Alias = "description")]
        public string? Description;

        [YamlMember(Alias = "tags")]
        public List<string>? Tags;

        [YamlMember(Alias = "authors")]
        public List<string>? Authors;

        [YamlMember(Alias = "inputs")]
        public Dictionary<string, dynamic> Inputs;

        [YamlMember(Alias = "parameters")]
        public Dictionary<string, dynamic> Parameters;

        [YamlMember(Alias = "model")]
        public PromptyModelConfig Model;

        [YamlMember(Alias = "api")]
        public ApiType? modelApiType;

        public string? Prompt { get; set; }
        public List<Dictionary<string, string>> Messages { get; set; }
        public ChatResponseMessage ChatResponseMessage { get; set; }
        public Completions CompletionResponseMessage { get; set; }
        public Embeddings EmbeddingResponseMessage { get; set; }
        public ImageGenerations ImageResponseMessage { get; set; }
        public List<ChatCompletionsToolCall> ToolCalls { get; set; }

        public string FilePath;

        // This is called from Execute to load a prompty file from location to create a Prompty object.
        // If sending a Prompty Object, this will not be used in execute.
        public Prompty Load(string promptyFileName, Prompty prompty)
        {
            //Check for appsettings.json config and set to that first
            prompty = Helpers.GetPromptyModelConfigFromSettings(prompty);

            //Then load settings from prompty file and override if not null
            var promptyFileInfo = new FileInfo(promptyFileName);

            // Get the full path of the prompty file
            prompty.FilePath = promptyFileInfo.FullName;
            var fileContent = File.ReadAllText(prompty.FilePath);
            // parse file in to frontmatter and prompty based on --- delimiter
            var promptyFrontMatterYaml = fileContent.Split("---")[1];
            var promptyContent = fileContent.Split("---")[2];
            // deserialize yaml into prompty object
            prompty = Helpers.ParsePromptyYamlFile(prompty, promptyFrontMatterYaml);
            prompty.Prompt = promptyContent;

            return prompty;
        }

        public async Task<Prompty> Execute(Prompty? prompty)
        {

            var render = new RenderPromptLiquidTemplate(prompty);
            render.RenderTemplate();

            // Parse
            var parser = new PromptyChatParser(prompty);
            parser.ParseTemplate(prompty);

            // Execute
            var executor = new AzureOpenAIExecutor(prompty);
            await executor.GetChatCompletiom(prompty);


            return prompty;
        }

    }
}