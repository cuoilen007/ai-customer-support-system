namespace AI.CustomerSupport.API.DTOs.Conversation
{
    public class ConversationResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
