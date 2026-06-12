using AI.CustomerSupport.API.Services.Interfaces;

namespace AI.CustomerSupport.API.Services
{
    public class OpenAIService : IOpenAIService
    {
        public async Task<string> AskAsync(string prompt)
        {
            await Task.Delay(100);

            return $"AI trả lời: {prompt}";
        }
    }
}
