using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Conversation;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConversationService _conversationService;

        public ConversationController(
            AppDbContext context, IConversationService conversationService)
        {
            _context = context;
            _conversationService = conversationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateConversationRequest request)
        {
            var userId =
                ClaimsHelper.GetUserId(User);

            var conversation =
                new Conversation
                {
                    UserId = userId,
                    Title = "New Conversation",
                    CreatedAt = DateTime.UtcNow
                };

            _context.Conversations.Add(conversation);

            await _context.SaveChangesAsync();

            return Ok(conversation);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId =
                ClaimsHelper.GetUserId(User);

            var conversations =
                await _context.Conversations
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new ConversationResponse
                    {
                        Id = x.Id,
                        Title = x.Title,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

            return Ok(conversations);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var conversation =
       await _context.Conversations
           .FirstOrDefaultAsync(
               x => x.Id == id
           );

            if (conversation == null)
            {
                throw new Exception(
                    "Conversation not found"
                );
            }

            _context.Conversations.Remove(
                conversation
            );

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/title")]
        public async Task<IActionResult>
UpdateTitle(
    int id,
    UpdateConversationTitleRequest request
)
        {
            await _conversationService
                .UpdateTitleAsync(
                    id,
                    request.Title 
                );

            return Ok();
        }
    }
}
