namespace AI.CustomerSupport.API.Services.Interfaces
{
    public interface IConversationService
    {
        Task UpdateTitleAsync(
    int conversationId,
    string title
);
    }
}
