using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Chat;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOpenAIService _openAIService;
        private readonly IAiService _aiService;

        public ChatController(AppDbContext context, IOpenAIService openAIService, IAiService aiService)
        {
            _context = context;
            _openAIService = openAIService;
            _aiService = aiService;
        }

        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> Send(ChatRequest request)
        {
            var userId =
                ClaimsHelper.GetUserId(User);

            var conversation =
                await _context.Conversations
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ConversationId
                        && x.UserId == userId);

            if (conversation == null)
            {
                return Forbid();
            }


            var answer = await _aiService.AskAsync(request.Message);

            var category = await _aiService
                    .ClassifyAsync(request.Message);

            var userMessage =
                new Message
                {
                    ConversationId = request.ConversationId,
                    Role = "user",
                    Content = request.Message,
                    CreatedAt = DateTime.UtcNow,
                    Category = category
                };

            _context.Messages.Add(userMessage);

            await _context.SaveChangesAsync();

            if (conversation.Title == "New Conversation")
                {
                    conversation.Title =
                        request.Message.Length > 40
                            ? request.Message.Substring(
                                0,
                                40
                              )
                            : request.Message;

                    await _context.SaveChangesAsync();
                }

            var assistantMessage =
                new Message
                {
                    ConversationId =
                        request.ConversationId,

                    Role = "assistant",

                    Content = answer.Answer,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.Messages.Add(assistantMessage);

            await _context.SaveChangesAsync();

            return Ok(
            new ChatResponse
            {
                Answer = answer.Answer
            });
        }
    }
}
