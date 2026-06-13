using AI.CustomerSupport.API.DTOs;
using AI.CustomerSupport.API.DTOs.AI;
using AI.CustomerSupport.API.Services.Interfaces;

namespace AI.CustomerSupport.API.Services
{
    public class AiService :  IAiService
    {
        private readonly HttpClient _httpClient;

        public AiService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AddDocumentAsync(
            string documentId,
            string content)
        {
            await _httpClient.PostAsJsonAsync(
                "/documents",
                new
                {
                    document_id = documentId,
                    content = content,
                    metadata = BuildMetadata(documentId)
                });
        }

        public async Task DeleteDocumentAsync(
            string documentId)
        {
            await _httpClient.DeleteAsync(
                $"/documents/{documentId}"
            );
        }

        public async Task<RagResponse> AskAsync(string question)
        {
            try
            {
                var response =
                await _httpClient
                    .PostAsJsonAsync(
                        "/rag",
                        new
                        {
                            question = question
                        });

            response.EnsureSuccessStatusCode();

            return await response
                .Content
                .ReadFromJsonAsync<RagResponse>()
                ?? new RagResponse();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(
                    "AI Service is not running. Please start FastAPI.",
                    ex);
            }
        }

        public async Task<bool> IsAiAliveAsync()
        {
            try
            {
                var response =
                    await _httpClient.GetAsync(
                        "/health"
                    );

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AiEvaluationResponse?> EvaluateAsync(
            string question,
            string answer,
            string context,
            string category
        )
        {
            try
            {
                var response =
                    await _httpClient.PostAsJsonAsync(
                        "/evaluate",
                        new
                        {
                            question,
                            answer,
                            context,
                            category
                        });

                response.EnsureSuccessStatusCode();

                return await response
                    .Content
                    .ReadFromJsonAsync<AiEvaluationResponse>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> ClassifyAsync(string text)
        {
            var response =
                await _httpClient
                .PostAsJsonAsync(
                    "/classify",
                    new
                    {
                        text
                    });

            var result =  await response.Content
                         .ReadFromJsonAsync<ClassificationResponse>();

            return result!.Category;
        }

        public async Task<AiTrainingStatusResponse?> GetTrainingStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/training/status");
                response.EnsureSuccessStatusCode();

                return await response.Content
                    .ReadFromJsonAsync<AiTrainingStatusResponse>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<AiTrainingStatusResponse?> RunTrainingAsync(
            AiTrainingRunRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/training/run",
                    request);

                response.EnsureSuccessStatusCode();

                return await response.Content
                    .ReadFromJsonAsync<AiTrainingStatusResponse>();
            }
            catch
            {
                return null;
            }
        }

        private static object BuildMetadata(
            string documentId)
        {
            var sourceType = "document";

            if (documentId.StartsWith(
                "product-",
                StringComparison.OrdinalIgnoreCase))
            {
                sourceType = "product";
            }

            if (documentId.StartsWith(
                "support-policy-",
                StringComparison.OrdinalIgnoreCase))
            {
                sourceType = "support_policy";
            }

            return new
            {
                source_id = documentId,
                source_type = sourceType
            };
        }
    }
}
