using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace ContosoChatAPI.Data;

public sealed class AISearchData(SearchClient searchClient)
{
    private readonly SearchClient _searchClient = searchClient;

    public async Task<List<Dictionary<string, string?>>> RetrieveDocumentationAsync(string question, ReadOnlyMemory<float> embedding)
    {
        var searchOptions = new SearchOptions
        {
            VectorSearch = new()
            {
                Queries = 
                {
                    new VectorizedQuery(embedding)
                    {
                        KNearestNeighborsCount = 3,
                        Fields = { "contentVector" }
                    } 
                }
            },
            Size = 3,
            Select = { "id", "title", "content", "url" },
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new()
            {
                SemanticConfigurationName = "default",
                QueryCaption = new(QueryCaptionType.Extractive),
                QueryAnswer = new(QueryAnswerType.Extractive),
            },
        };

        var results = await _searchClient.SearchAsync<SearchDocument>(question, searchOptions);

        var docs = new List<Dictionary<string, string?>>();
        await foreach (var doc in results.Value.GetResultsAsync())
        {
            docs.Add(new()
            {
                { "id", doc.Document["id"].ToString() },
                { "title", doc.Document["title"].ToString() },
                { "content", doc.Document["content"].ToString() },
                { "url", doc.Document["url"].ToString() }
            });
        }

        return docs;
    }
}
