using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Document;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiService _aiService;

        public DocumentController(
            AppDbContext context,
            IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var documents = await _context.Documents
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDocumentRequest request)
        {
            var document = new Document
            {
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);

            await _context.SaveChangesAsync();
            await _aiService.AddDocumentAsync(
                KnowledgeContentBuilder.GetDocumentVectorId(document.Id),
                KnowledgeContentBuilder.BuildDocumentContent(document));

            return Ok(document);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateDocumentRequest request)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            document.Title = request.Title;
            document.Content = request.Content;

            await _context.SaveChangesAsync();
            await _aiService.DeleteDocumentAsync(
                KnowledgeContentBuilder.GetDocumentVectorId(document.Id));

            await _aiService.AddDocumentAsync(
                KnowledgeContentBuilder.GetDocumentVectorId(document.Id),
                KnowledgeContentBuilder.BuildDocumentContent(document));

            return Ok(document);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(document);

            await _context.SaveChangesAsync();
            await _aiService.DeleteDocumentAsync(
                KnowledgeContentBuilder.GetDocumentVectorId(document.Id));

            return NoContent();
        }
    }
}
