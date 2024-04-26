using global::Prompty.Core.Types;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace Prompty.Core
{

    public static class Helpers
    {
        // This is to load the appsettings.json file config
        // These are the base configuration settings for the prompty file
        // These can be overriden by the prompty file, or the execute method
        public static Prompty GetPromptyModelConfigFromSettings(Prompty prompty)
        {
            //TODO: default prompty json, can have multiple sections, need to loop thru sections?
            //TODO: account for multiple prompty.json files
            // Get the connection string from appsettings.json
            var config = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json")
                            .AddEnvironmentVariables()
                            .Build();

            var section = config.GetSection("Prompty");
            // get variables from section and assign to promptymodelconfig
            var promptyModelConfig = new PromptyModelConfig();
            if (section != null)
            {
                var type = section["type"];
                var apiVersion = section["api_version"];
                var azureEndpoint = section["azure_endpoint"];
                var azureDeployment = section["azure_deployment"];
                var apiKey = section["api_key"];
                if (type != null)
                {
                    //parse type to ModelType enum
                    promptyModelConfig.ModelType = (ModelType)Enum.Parse(typeof(ModelType), type);

                }
                if (apiVersion != null)
                {
                    promptyModelConfig.ApiVersion = apiVersion;
                }
                if (azureEndpoint != null)
                {
                    promptyModelConfig.AzureEndpoint = azureEndpoint;
                }
                if (azureDeployment != null)
                {
                    promptyModelConfig.AzureDeployment = azureDeployment;
                }
                if (apiKey != null)
                {
                    promptyModelConfig.ApiKey = apiKey;
                }

                prompty.Model = promptyModelConfig;
            }

            return prompty;
        }


        public static Prompty ParsePromptyYamlFile(Prompty prompty, string promptyFrontMatterYaml)
        {
            // desearialize yaml front matter
            // TODO: check yaml to see what props are missing? update to include template type, update so invoker descides based on prop
            var deserializer = new DeserializerBuilder().Build();
            var promptyFrontMatter = deserializer.Deserialize<Prompty>(promptyFrontMatterYaml);

            // override props if they are not null from file
            if (promptyFrontMatter.Name != null)
            {
                // check each prop and if not null override
                if (promptyFrontMatter.Name != null)
                {
                    prompty.Name = promptyFrontMatter.Name;
                }
                if (promptyFrontMatter.Description != null)
                {
                    prompty.Description = promptyFrontMatter.Description;
                }
                if (promptyFrontMatter.Tags != null)
                {
                    prompty.Tags = promptyFrontMatter.Tags;
                }
                if (promptyFrontMatter.Authors != null)
                {
                    prompty.Authors = promptyFrontMatter.Authors;
                }
                if (promptyFrontMatter.Inputs != null)
                {
                    prompty.Inputs = promptyFrontMatter.Inputs;
                }
                if (promptyFrontMatter.Parameters != null)
                {
                    prompty.Parameters = promptyFrontMatter.Parameters;
                }
                if (promptyFrontMatter.modelApiType != null)
                {
                    //parse type to enum
                    prompty.modelApiType = promptyFrontMatter.modelApiType;
                }
                if (promptyFrontMatter.Model != null)
                {
                    //check for each prop of promptymodelconfig and override if not null
                    if (promptyFrontMatter.Model.ModelType != null)
                    {
                        //parse type to enum
                        prompty.Model.ModelType = promptyFrontMatter.Model.ModelType;
                    }
                    if (promptyFrontMatter.Model.ApiVersion != null)
                    {
                        prompty.Model.ApiVersion = promptyFrontMatter.Model.ApiVersion;
                    }
                    if (promptyFrontMatter.Model.AzureEndpoint != null)
                    {
                        prompty.Model.AzureEndpoint = promptyFrontMatter.Model.AzureEndpoint;
                    }
                    if (promptyFrontMatter.Model.AzureDeployment != null)
                    {
                        prompty.Model.AzureDeployment = promptyFrontMatter.Model.AzureDeployment;
                    }
                    if (promptyFrontMatter.Model.ApiKey != null)
                    {
                        prompty.Model.ApiKey = promptyFrontMatter.Model.ApiKey;
                    }
                }

            }

            return prompty;

        }

    }
}