using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.Data;
using Online_Store_Backend.DTOs.Product;
using Online_Store_Backend.ResponseDto.Product;
using Online_Store_Backend.Table; 

namespace Online_Store_Backend.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("products")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            return Ok(products);
        }

        // 4. جلب تفاصيل منتج معين
        [HttpGet("product/{id}/details")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // 1. إضافة منتج جديد مع مخزونه الأولي
        [HttpPost("createProduct")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                QuantityInStock = dto.InitialQuantity,
                CostPrice = dto.UnitCostPrice,
                SellingPrice = dto.SellingPrice,
                MinQuantityAlert = dto.MinQuantityAlert
            };

            var purchase = new Purchase
            {
                ProductId = product.Id,
                Quantity = dto.InitialQuantity,
                UnitCostPrice = dto.UnitCostPrice,
                CreatedByUserId = userId
            };

            await _context.Products.AddAsync(product);
            await _context.Purchases.AddAsync(purchase);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // 2. تعديل سعر البيع لمنتج موجود
        [HttpPatch("updateProduct/{id}/price")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(UpdatePriceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSellingPrice(string id, [FromBody] UpdatePriceDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("المنتج غير موجود.");

            product.SellingPrice = dto.NewSellingPrice;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تحديث سعر البيع بنجاح.", ProductId = id, NewPrice = product.SellingPrice });
        }

        // 3. إضافة مخزون جديد لمنتج موجود
        [HttpPost("product/{id}/add-stock")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(AddStockResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddStock(string id, [FromBody] AddStockDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("المنتج غير موجود.");


            int totalQuantity = product.QuantityInStock + dto.Quantity;

            product.CostPrice = dto.UnitCostPrice;
            product.QuantityInStock = totalQuantity;

            var purchase = new Purchase
            {
                ProductId = product.Id,
                Quantity = dto.Quantity,
                UnitCostPrice = dto.UnitCostPrice,
                CreatedByUserId = userId
            };

            await _context.Purchases.AddAsync(purchase);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تمت إضافة المخزون وتحديث متوسط التكلفة بنجاح.", product });
        }

       
    }
}