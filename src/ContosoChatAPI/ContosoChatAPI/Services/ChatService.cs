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
    private readonly KernelFunction _chat = kernel.CreateFunctionFromPromptyFile("chat.prompty");
    private readonly KernelFunction _groudedness = kernel.CreateFunctionFromPromptyFile(Path.Combine("Evaluations", "groundedness.prompty"));
    private readonly KernelFunction _coherence = kernel.CreateFunctionFromPromptyFile(Path.Combine("Evaluations", "coherence.prompty"));
    private readonly KernelFunction _relevance = kernel.CreateFunctionFromPromptyFile(Path.Combine("Evaluations", "relevance.prompty"));
    private readonly KernelFunction _fluency = kernel.CreateFunctionFromPromptyFile(Path.Combine("Evaluations", "fluency.prompty"));

    public async Task<string> GetResponseAsync(string customerId, string question)
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
        });


        _logger.LogInformation("Answer: {Answer}", answer);

        return JsonSerializer.Serialize(new { answer, context });
    }

    // Evaluate the answer using the specified function.
    public async Task<Dictionary<string, int?>> GetEvaluationAsync(string question, string context, string answer)
    {
        _logger.LogInformation("Evaluating result.");
        var groundednessEvaluation = Evaluate(_groudedness, question, context, answer);
        var coherenceEvaluation = Evaluate(_coherence, question, context, answer);
        var relevanceEvaluation = Evaluate(_relevance, question, context, answer);
        var fluencyEvaluation = Evaluate(_fluency, question, context, answer);

        var score = new Dictionary<string, int?>
        {
            ["groundedness"] = await groundednessEvaluation,
            ["coherence"] = await coherenceEvaluation,
            ["relevance"] = await relevanceEvaluation,
            ["fluency"] = await fluencyEvaluation,
        };

        await Task.WhenAll(groundednessEvaluation, coherenceEvaluation, relevanceEvaluation, fluencyEvaluation);
        _logger.LogInformation("Score: {Score}", score);
        return score;
    }

    private Task<int?> Evaluate(KernelFunction func, string question, object context, string? answer)
    {
        return func.InvokeAsync<int?>(_kernel, new()
        {
            { "question", question },
            { "context", context },
            { "answer", answer },
        });
    }
}