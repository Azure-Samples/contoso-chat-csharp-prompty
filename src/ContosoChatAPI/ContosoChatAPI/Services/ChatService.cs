using ContosoChatAPI.Data;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;

namespace ContosoChatAPI.Services;

public sealed class ChatService(Kernel kernel, ITextEmbeddingGenerationService embedding, CustomerData customerData, AISearchData aiSearch, ILogger<ChatService> logger)
{
    private readonly CustomerData _customerData = customerData;
    private readonly AISearchData _aiSearch = aiSearch;
    private readonly ILogger<ChatService> _logger = logger;

    private readonly Kernel _kernel = kernel;
    private readonly ITextEmbeddingGenerationService _embedding = embedding;
    private readonly KernelFunction _chat = kernel.CreateFunctionFromPrompty("chat.prompty");
    private readonly KernelFunction _groudedness = kernel.CreateFunctionFromPrompty(Path.Combine("Evaluations", "groundedness.prompty"));
    private readonly KernelFunction _coherence = kernel.CreateFunctionFromPrompty(Path.Combine("Evaluations", "coherence.prompty"));
    private readonly KernelFunction _relevance = kernel.CreateFunctionFromPrompty(Path.Combine("Evaluations", "relevance.prompty"));
    private readonly KernelFunction _fluency = kernel.CreateFunctionFromPrompty(Path.Combine("Evaluations", "fluency.prompty"));

    public async Task<string> GetResponseAsync(string customerId, string question, List<string> chatHistory)
    {
        _logger.LogInformation("CustomerId = {CustomerID}, Question = {Question}", customerId, question);

        var customer = await _customerData.GetCustomerAsync(customerId);
        var embedding = await _embedding.GenerateEmbeddingAsync(question);
        var context = await _aiSearch.RetrieveDocumentationAsync(question, embedding);

        _logger.LogInformation("Getting result.");
        string? answer = await _chat.InvokeAsync<string>(_kernel, new()
        {
            { "customer", customer },
            { "documentation", context },
            { "question", question },
            { "chatHistory", chatHistory }
        });

        _logger.LogInformation("Evaluating result.");
        var score = new Dictionary<string, string?>
        {
            ["groundedness"] = await Evaluate(_groudedness, question, context, answer),
            ["coherence"] = await Evaluate(_coherence, question, context, answer),
            ["relevance"] = await Evaluate(_relevance, question, context, answer),
            ["fluency"] = await Evaluate(_fluency, question, context, answer),
        };

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Result: {Result}", answer);
            _logger.LogInformation("Score: {Score}", string.Join(", ", score));
        }

        return JsonSerializer.Serialize(new { answer, score });
    }

    private Task<string?> Evaluate(KernelFunction func, string question, object context, string? answer)
    {
        return func.InvokeAsync<string>(_kernel, new()
        {
            { "question", question },
            { "answer", answer },
            { "context", context },
        });
    }
}