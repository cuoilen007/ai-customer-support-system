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


        Task<bool> IsAiAliveAsync();
    }
}
