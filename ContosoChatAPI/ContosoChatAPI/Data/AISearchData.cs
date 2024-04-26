using System;
using System.Collections.Generic;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents.Indexes.Models;
using System.Globalization;

namespace ContosoChatAPI.Data
{
    public class AISearchData
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;
        readonly string _indexName;

        public AISearchData(IConfiguration config, SearchClient searchClient, SearchIndexClient searchIndexClient)
        {
            _indexName = config["AzureAISearch:index_name"];
            _searchClient = searchClient;
            _searchIndexClient = searchIndexClient;
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
