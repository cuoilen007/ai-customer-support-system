using AI.CustomerSupport.API.DTOs.AI;

namespace AI.CustomerSupport.API.Services.Interfaces
{
    public interface IAiService
    {
        Task AddDocumentAsync(
           string documentId,
           string content
       );

        Task DeleteDocumentAsync(
            string documentId
        );

        Task<RagResponse> AskAsync(string question);

        Task<AiEvaluationResponse?> EvaluateAsync(
            string question,
            string answer,
            string context,
            string category
        );


        Task<bool> IsAiAliveAsync();

        Task<string> ClassifyAsync(
            string text
        );

        Task<AiTrainingStatusResponse?> GetTrainingStatusAsync();

        Task<AiTrainingStatusResponse?> RunTrainingAsync(
            AiTrainingRunRequest request
        );
    }
}
