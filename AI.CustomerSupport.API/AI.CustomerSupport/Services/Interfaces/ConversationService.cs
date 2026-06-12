using AI.CustomerSupport.API.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Services.Interfaces
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;
        public ConversationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateTitleAsync(
    int conversationId,
    string title
)
        {
            var conversation =
                await _context.Conversations
                    .FirstOrDefaultAsync(
                        x => x.Id == conversationId
                    );

            if (conversation == null)
            {
                throw new Exception(
                    "Conversation not found"
                );
            }

            conversation.Title = title;

            await _context.SaveChangesAsync();
        }
    }
}
