using Microsoft.Azure.Cosmos;
using Azure.Identity;
using ContosoChatAPI.Data;
using Azure.Search.Documents;
using ContosoChatAPI.Services;
using ContosoChatAPI.Evaluations;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace ContosoChatAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                return new CosmosClient(builder.Configuration["CosmosDb:Endpoint"], new DefaultAzureCredential());
            });

            builder.Services.AddSingleton<OpenAIClient>(serviceProvider =>
            {
                return new OpenAIClient(new Uri(builder.Configuration["OpenAi:endpoint"]), new DefaultAzureCredential());
            });

            builder.Services.AddSingleton<SearchClient>(serviceProvider =>
            {
                return new SearchClient(
                                new Uri(builder.Configuration["AzureAISearch:Endpoint"]),
                                builder.Configuration["AzureAISearch:index_name"],
                                new DefaultAzureCredential());
            });

            builder.Services.AddSingleton<SearchIndexClient>(serviceProvider =>
            {
                return new SearchIndexClient(
                                new Uri(builder.Configuration["AzureAISearch:Endpoint"]),
                                new DefaultAzureCredential());
            });

            builder.Services.AddScoped<CustomerData>();
            builder.Services.AddSingleton<GenerateCustomerInfo>();
            builder.Services.AddSingleton<GenerateProductInfo>();
            builder.Services.AddScoped<EmbeddingData>();
            builder.Services.AddScoped<AISearchData>();
            builder.Services.AddScoped<Evaluation>();
            builder.Services.AddScoped<ChatService>();

            //Application Insights
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
                options.Credential = new DefaultAzureCredential();
            });

            var app = builder.Build();

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                //Populate CosmosDB and AI Search with sample data
                var customerData = app.Services.GetRequiredService<GenerateCustomerInfo>();
                var aiSearchData = app.Services.GetRequiredService<GenerateProductInfo>();

                _ = customerData.PopulateCosmosAsync();
                _ = aiSearchData.PopulateSearchIndexAsync();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}