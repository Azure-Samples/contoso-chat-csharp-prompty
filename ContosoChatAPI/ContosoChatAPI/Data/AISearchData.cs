using System;
using System.Collections.Generic;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Core;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;


namespace ContosoChatAPI.Data
{
    public class AISearchData
    {
        private readonly AzureKeyCredential _credentials;
        private readonly SearchClient _searchClient;



        public AISearchData(string searchEndpoint, string searchKey, string indexName)
        {
            _searchClient = new SearchClient(
                                new Uri(searchEndpoint),
                                indexName,
                                new DefaultAzureCredential());

        }
        public async Task<List<Dictionary<string, string>>> RetrieveDocumentationAsync(
                string question,
                Embeddings embedding)
        {
            var searchOptions = new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = {
                        new VectorizedQuery(embedding.Data[0].Embedding.ToArray()) {
                            KNearestNeighborsCount = 3,
                            Fields = { "contentVector" } } }
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

            var docs = new List<Dictionary<string, string>>();
            await foreach (var doc in results.Value.GetResultsAsync())
            {
                var docInfo = new Dictionary<string, string>
                {
                    { "id", doc.Document["id"].ToString() },
                    { "title", doc.Document["title"].ToString() },
                    { "content", doc.Document["content"].ToString() },
                    { "url", doc.Document["url"].ToString() }
                };
                docs.Add(docInfo);
            }

            return docs;
        }
    }
}
