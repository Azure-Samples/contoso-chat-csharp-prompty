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

        var customerTask = _customerData.GetCustomerAsync(customerId);
        var embeddingTask = _embedding.GenerateEmbeddingAsync(question);
        await Task.WhenAll(customerTask, embeddingTask);

        var customer = await customerTask;
        var embedding = await embeddingTask;

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


        var groundedNessEvaluation = Evaluate(_groudedness, question, context, answer);
        var coherenceEvaluation = Evaluate(_coherence, question, context, answer);
        var relevanceEvaluation = Evaluate(_relevance, question, context, answer);
        var fluencyEvaluation = Evaluate(_fluency, question, context, answer);
        await Task.WhenAll(groundedNessEvaluation, coherenceEvaluation, relevanceEvaluation, fluencyEvaluation);

        var score = new Dictionary<string, string?>
        {
            ["groundedness"] = await groundedNessEvaluation,
            ["coherence"] = await coherenceEvaluation,
            ["relevance"] = await relevanceEvaluation,
            ["fluency"] = await fluencyEvaluation,
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