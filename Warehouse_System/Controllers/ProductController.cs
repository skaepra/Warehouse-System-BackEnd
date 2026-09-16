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

        // 1. جلب قائمة المنتجات مع تضمين التصنيف المرتبط
        [HttpGet("products")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts()
        {
            // استخدام Select لتحويل البيانات للشكل المسطح
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "غير محدد", // استخراج الاسم هنا
                    QuantityInStock = p.QuantityInStock,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    MinQuantityAlert = p.MinQuantityAlert,
                    IsActive = p.IsActive
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
        }

        // 2. جلب تفاصيل منتج معين
        [HttpGet("product/{id}/details")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(string id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "غير محدد",
                    QuantityInStock = p.QuantityInStock,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    MinQuantityAlert = p.MinQuantityAlert,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // 3. إضافة منتج جديد مع مخزونه الأولي
        [HttpPost("createProduct")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
            {
                return BadRequest("التصنيف المرفق غير موجود في النظام.");
            }

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                CategoryId = dto.CategoryId,
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

            // تحضير الـ DTO للإرجاع
            var responseDto = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                QuantityInStock = product.QuantityInStock,
                CostPrice = product.CostPrice,
                SellingPrice = product.SellingPrice,
                MinQuantityAlert = product.MinQuantityAlert,
                IsActive = product.IsActive
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, responseDto);
        }

        // 4. تعديل سعر البيع لمنتج موجود
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

        // 5. إضافة مخزون جديد لمنتج موجود
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

            return Ok(new { Message = "تمت إضافة المخزون بنجاح.", product });
        }
    }
}