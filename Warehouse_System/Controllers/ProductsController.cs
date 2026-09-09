using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.Data;
using Online_Store_Backend.DTOs;

namespace Online_Store_Backend.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class ProductsController : ControllerBase
    //{
    //    private readonly ApplicationDbContext _context;

    //    public ProductsController(ApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    // GET: api/Products
    //    [HttpGet]
    //    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    //    {
    //        return await _context.Products
    //            .Include(p => p.Variants)
    //            .AsNoTracking()
    //            .ToListAsync();
    //    }

    //    // GET: api/Products/5
    //    [HttpGet("{id}")]
    //    public async Task<ActionResult<Product>> GetProduct(string id)
    //    {
    //        var product = await _context.Products
    //            .Include(p => p.Variants)
    //            .AsNoTracking()
    //            .FirstOrDefaultAsync(p => p.Id == id);

    //        if (product == null)
    //        {
    //            return NotFound(new { message = $"Product with ID {id} not found." });
    //        }

    //        return product;
    //    }

    //    // POST: api/Products (إنشاء منتج باستخدام CreateProductDto)
    //    [HttpPost]
    //    [ProducesResponseType(typeof(CreateProductResponseDto), StatusCodes.Status201Created)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        // تحويل الـ DTO إلى Entity وتوليد الـ IDs تلقائياً في السيرفر
    //        var product = new Product
    //        {
    //            Id = Guid.NewGuid().ToString(),
    //            Name = dto.Name,
    //            BasePrice = dto.BasePrice,
    //            ImageAlt = dto.ImageAlt,
    //            Description = dto.Description,
    //            Category = dto.Category,
    //            IsFeatured = dto.IsFeatured,
    //            DefaultImages = dto.DefaultImages,
    //            Variants = dto.Variants.Select(v => new ProductVariant
    //            {
    //                Id = Guid.NewGuid().ToString(),
    //                Color = v.Color,
    //                Size = v.Size,
    //                StockQuantity = v.StockQuantity,
    //                Images = v.Images
    //            }).ToList()
    //        };

    //        _context.Products.Add(product);
    //        await _context.SaveChangesAsync();

    //        return StatusCode(StatusCodes.Status201Created, new {  id = product.Id });
    //    }

    //    // PUT: api/Products/5 (تحديث منتج باستخدام UpdateProductDto)
    //    [HttpPut("{id}")]
    //    public async Task<IActionResult> UpdateProduct(string id, UpdateProductDto dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var existingProduct = await _context.Products.FindAsync(id);


    //        if (existingProduct == null)
    //        {
    //            return NotFound(new { message = $"Product with ID {id} not found." });
    //        }

    //        // تحديث الحقول الأساسية
    //        existingProduct.Name = dto.Name;
    //        existingProduct.BasePrice = dto.BasePrice;
    //        existingProduct.Description = dto.Description;
    //        existingProduct.Category = dto.Category;
    //        existingProduct.ImageAlt = dto.ImageAlt;
    //        existingProduct.IsFeatured = dto.IsFeatured;
    //        existingProduct.DefaultImages = dto.DefaultImages;

    //        await _context.SaveChangesAsync();

    //        return NoContent();
    //    }

    //    // DELETE: api/Products/5
    //    [HttpDelete("{id}")]
    //    public async Task<IActionResult> DeleteProduct(string id)
    //    {
    //        var product = await _context.Products.FindAsync(id);
    //        if (product == null)
    //        {
    //            return NotFound(new { message = $"Product with ID {id} not found." });
    //        }

    //        _context.Products.Remove(product);
    //        await _context.SaveChangesAsync();

    //        return NoContent();
    //    }
    //}
}