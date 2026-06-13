using AI.CustomerSupport.API.DTOs.AI;
using AI.CustomerSupport.API.Services.Interfaces;

namespace AI.CustomerSupport.Tests.TestSupport
{
    internal class FakeAiService : IAiService
    {
        public int AddedDocumentCount { get; private set; }

        public RagResponse AskResponse { get; set; } = new();

        public AiEvaluationResponse? EvaluationResponse { get; set; }

        public string ClassificationResponse { get; set; } = "General";

        public Task AddDocumentAsync(string documentId, string content)
        {
            AddedDocumentCount++;
            return Task.CompletedTask;
        }

        public Task DeleteDocumentAsync(string documentId)
        {
            return Task.CompletedTask;
        }

        public Task<RagResponse> AskAsync(string question)
        {
            return Task.FromResult(AskResponse);
        }

        public Task<AiEvaluationResponse?> EvaluateAsync(
            string question,
            string answer,
            string context,
            string category)
        {
            return Task.FromResult(EvaluationResponse);
        }

        public Task<bool> IsAiAliveAsync()
        {
            return Task.FromResult(true);
        }

        public Task<string> ClassifyAsync(string text)
        {
            return Task.FromResult(ClassificationResponse);
        }
    }
}
