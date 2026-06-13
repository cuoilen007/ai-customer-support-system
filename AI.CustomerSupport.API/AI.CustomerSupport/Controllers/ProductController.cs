using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Product;
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
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiService _aiService;

        public ProductController(AppDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Price = request.Price,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _aiService.AddDocumentAsync(
                KnowledgeContentBuilder.GetProductVectorId(product.Id),
                KnowledgeContentBuilder.BuildProductContent(product));

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductRequest request)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Category = request.Category;
            product.Price = request.Price;
            product.Status = request.Status;

            await _context.SaveChangesAsync();

            await _aiService.DeleteDocumentAsync(
                KnowledgeContentBuilder.GetProductVectorId(product.Id));
            await _aiService.AddDocumentAsync(
                KnowledgeContentBuilder.GetProductVectorId(product.Id),
                KnowledgeContentBuilder.BuildProductContent(product));

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            await _aiService.DeleteDocumentAsync(
                KnowledgeContentBuilder.GetProductVectorId(product.Id));

            return NoContent();
        }
    }
}
