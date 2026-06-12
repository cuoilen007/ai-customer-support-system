namespace AI.CustomerSupport.API.Services.Interfaces
{
    public interface IOpenAIService
    {
        Task<string> AskAsync(string prompt);
    }
}
