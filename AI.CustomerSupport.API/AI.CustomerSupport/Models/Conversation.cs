namespace AI.CustomerSupport.API.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}
