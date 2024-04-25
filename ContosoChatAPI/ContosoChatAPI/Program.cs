using Microsoft.Azure.Cosmos;
using Azure.Identity;
using System.Configuration;
using ContosoChatAPI.Data;
using Azure.Search.Documents;

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

            builder.Services.AddSingleton<CosmosClient>(serviceProvider => {
                return new CosmosClient(builder.Configuration["CosmosDb:Endpoint"], new DefaultAzureCredential());
            });

            builder.Services.AddScoped<CustomerData>();
            builder.Services.AddScoped<EmbeddingData>();
            builder.Services.AddScoped<AISearchData>();

            var app = builder.Build();

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

            //TODO: Add code to populate Cosmos (asynchrhonous)
        }
    }
}