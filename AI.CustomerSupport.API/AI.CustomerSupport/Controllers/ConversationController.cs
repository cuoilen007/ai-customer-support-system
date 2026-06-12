using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Conversation;
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
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConversationController(
            AppDbContext context)
        {
            _context = context;
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
                    Title = request.Title,
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
    }
}
