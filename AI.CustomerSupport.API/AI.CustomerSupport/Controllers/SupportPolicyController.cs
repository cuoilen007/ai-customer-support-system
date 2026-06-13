using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.SupportPolicy;
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
    public class SupportPolicyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiService _aiService;

        public SupportPolicyController(AppDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var policies = await _context.SupportPolicies
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(policies);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSupportPolicyRequest request)
        {
            var policy = new SupportPolicy
            {
                Title = request.Title,
                PolicyType = request.PolicyType,
                Content = request.Content,
                EffectiveFrom = request.EffectiveFrom,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportPolicies.Add(policy);
            await _context.SaveChangesAsync();

            await _aiService.AddDocumentAsync(
                KnowledgeContentBuilder.GetSupportPolicyVectorId(policy.Id),
                KnowledgeContentBuilder.BuildSupportPolicyContent(policy));

            return Ok(policy);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSupportPolicyRequest request)
        {
            var policy = await _context.SupportPolicies.FirstOrDefaultAsync(x => x.Id == id);

            if (policy == null)
            {
                return NotFound();
            }

            policy.Title = request.Title;
            policy.PolicyType = request.PolicyType;
            policy.Content = request.Content;
            policy.EffectiveFrom = request.EffectiveFrom;

            await _context.SaveChangesAsync();

            await _aiService.DeleteDocumentAsync(
                KnowledgeContentBuilder.GetSupportPolicyVectorId(policy.Id));
            await _aiService.AddDocumentAsync(
                KnowledgeContentBuilder.GetSupportPolicyVectorId(policy.Id),
                KnowledgeContentBuilder.BuildSupportPolicyContent(policy));

            return Ok(policy);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _context.SupportPolicies.FirstOrDefaultAsync(x => x.Id == id);

            if (policy == null)
            {
                return NotFound();
            }

            _context.SupportPolicies.Remove(policy);
            await _context.SaveChangesAsync();
            await _aiService.DeleteDocumentAsync(
                KnowledgeContentBuilder.GetSupportPolicyVectorId(policy.Id));

            return NoContent();
        }
    }
}
