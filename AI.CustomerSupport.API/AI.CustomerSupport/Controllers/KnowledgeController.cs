using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KnowledgeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiService _aiService;

        public KnowledgeController(
            AppDbContext context,
            IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpPost("reindex-all")]
        public async Task<IActionResult> ReindexAll()
        {
            var documents = await _context.Documents
                .AsNoTracking()
                .ToListAsync();

            var products = await _context.Products
                .AsNoTracking()
                .ToListAsync();

            var supportPolicies = await _context.SupportPolicies
                .AsNoTracking()
                .ToListAsync();

            foreach (var document in documents)
            {
                await _aiService.AddDocumentAsync(
                    KnowledgeContentBuilder.GetDocumentVectorId(document.Id),
                    KnowledgeContentBuilder.BuildDocumentContent(document));
            }

            foreach (var product in products)
            {
                await _aiService.AddDocumentAsync(
                    KnowledgeContentBuilder.GetProductVectorId(product.Id),
                    KnowledgeContentBuilder.BuildProductContent(product));
            }

            foreach (var policy in supportPolicies)
            {
                await _aiService.AddDocumentAsync(
                    KnowledgeContentBuilder.GetSupportPolicyVectorId(policy.Id),
                    KnowledgeContentBuilder.BuildSupportPolicyContent(policy));
            }

            return Ok(new
            {
                documents = documents.Count,
                products = products.Count,
                supportPolicies = supportPolicies.Count,
                total = documents.Count + products.Count + supportPolicies.Count
            });
        }
    }
}
