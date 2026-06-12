using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Message;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageController(
            AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("{conversationId}")]
        public async Task<IActionResult> SendMessage(
    int conversationId,
    SendMessageRequest request)
        {
            if (!await IsOwner(conversationId))
            {
                return Forbid();
            }

            var conversation =
                await _context.Conversations
                    .FirstOrDefaultAsync(x =>
                        x.Id == conversationId);

            if (conversation == null)
            {
                return NotFound();
            }

            var message =
                new Message
                {
                    ConversationId = conversationId,
                    Role = "user",
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow
                };

            _context.Messages.Add(message);

            await _context.SaveChangesAsync();

            return Ok(message);
        }


        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(
    int conversationId)
        {
            if (!await IsOwner(conversationId))
            {
                return Forbid();
            }

            var conversation =
                await _context.Conversations
                    .FirstOrDefaultAsync(x =>
                        x.Id == conversationId);

            if (conversation == null)
            {
                return NotFound();
            }


            var messages =
              await _context.Messages
                 .Where(x => x.ConversationId == conversationId)
                 .OrderBy(x => x.CreatedAt)
                 .Select(x => new MessageResponse
                 {
                     Id = x.Id,
                     Role = x.Role,
                     Content = x.Content,
                     CreatedAt = x.CreatedAt
                 })
                 .ToListAsync();

                return Ok(messages);
        }

        private async Task<bool> IsOwner(
            int conversationId)
        {
            var userId =
                ClaimsHelper.GetUserId(User);

            return await _context.Conversations
                .AnyAsync(x =>
                    x.Id == conversationId &&
                    x.UserId == userId);
        }
    }
}
