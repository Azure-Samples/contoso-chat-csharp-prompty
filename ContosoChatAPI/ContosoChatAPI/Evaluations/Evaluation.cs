using Newtonsoft.Json.Linq;

namespace ContosoChatAPI.Evaluations
{
    public static class Evaluation
    {
        // Run a batch coherence evaluation
        public static async Task<List<string>> Batch(string file, string path)
        {
            if(!File.Exists(file))
            {
                file =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.jsonl");
            }

            var results = new List<string>();
            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var data = JObject.Parse(line);
                var result = await Evaluate(data["question"].ToString(), data["context"], data["answer"].ToString(), path);
                results.Add(result);
            }

            return results;
        }

        // Run a single coherence evaluation
        public static async Task<string> Evaluate(string question, object context, string answer, string path)
        {
            var inputs = new Dictionary<string, dynamic>
            {
                { "answer", answer },
                { "context", context },
                { "question", question }
            };


            // Replace this with your actual coherence evaluation logic
            // For demonstration purposes, I'll return a placeholder result.
            return "result";
        }
    }
}
