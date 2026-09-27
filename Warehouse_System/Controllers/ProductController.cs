using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse_System_BackEnd.Data;
using Warehouse_System_BackEnd.DTOs.Product;
using Warehouse_System_BackEnd.ResponseDto.Product;
using Warehouse_System_BackEnd.Table;

namespace Warehouse_System_BackEnd.Controllers
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
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var trimmedName = dto.Name.Trim();

            // 1. التحقق من عدم تكرار الاسم
            var isNameExists = await _context.Products
                .AnyAsync(p => p.Name.ToLower() == trimmedName.ToLower());

            if (isNameExists)
                return BadRequest(new { message = "يوجد منتج آخر مسجل بنفس هذا الاسم بالفعل." });

            // 2. معالجة الـ SKU (توليد تلقائي من 6 محارف إذا كان فارغاً)
            string finalSku;
            if (string.IsNullOrWhiteSpace(dto.SKU))
            {
                do
                {
                    finalSku = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                }
                while (await _context.Products.AnyAsync(p => p.SKU == finalSku));
            }
            else
            {
                finalSku = dto.SKU.Trim().ToUpper();

                var isSkuExists = await _context.Products.AnyAsync(p => p.SKU == finalSku);
                if (isSkuExists)
                    return BadRequest(new { message = "رمز الSKU المستخدم يخص منتج أخر." });
            }

            // 3. التحقق من وجود التصنيف
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest("التصنيف المرفق غير موجود في النظام.");

            // 4. إنشاء كيان المنتج الأسعار والمخزون تبدأ
            var product = new Product
            {
                Name = trimmedName,
                SKU = finalSku,
                CategoryId = dto.CategoryId,
                MinQuantityAlert = dto.MinQuantityAlert,
                QuantityInStock = 0,
                CostPrice = 0,
                SellingPrice = 0
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // 5. تحضير الـ DTO للإرجاع
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
        [Authorize(Roles = "Manager, SalesRepresentative")]
        [ProducesResponseType(typeof(AddStockResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddStock(string id, [FromBody] AddStockDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 1. التحقق من المدخلات الرقمية
            if (dto.Quantity <= 0)
                return BadRequest("الكمية المضافة يجب أن تكون أكبر من الصفر.");

            if (dto.UnitCostPrice <= 0)
                return BadRequest("سعر الشراء يجب أن يكون أكبر من الصفر.");

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("المنتج غير موجود.");

            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId);
            if (!supplierExists) return BadRequest("المورد المحدد غير موجود.");

            // 2. التحقق إن كان المورد مسجلاً سابقاً للمنتج
            var isSupplierLinked = await _context.ProductSuppliers
                .AnyAsync(ps => ps.ProductId == id && ps.SupplierId == dto.SupplierId);

            if (!isSupplierLinked) return BadRequest("المورد الذي تم اختياره غير مرتبط بهذا المنتج.");

            // 3. بدء المعاملة (Transaction) لضمان اتساق البيانات بين الجدولين
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                product.QuantityInStock += dto.Quantity;
                product.CostPrice = dto.UnitCostPrice;

                // حساب سعر البيع تلقائياً بزادة 20
                product.SellingPrice = dto.UnitCostPrice * 1.20m;

                // 5. إنشاء سجل حركة الشراء 
                var purchase = new Purchase
                {
                    ProductId = product.Id,
                    SupplierId = dto.SupplierId,
                    Quantity = dto.Quantity,
                    UnitCostPrice = dto.UnitCostPrice,
                    PurchaseDate = DateTime.UtcNow,
                    CreatedByUserId = userId
                };

                await _context.Purchases.AddAsync(purchase);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 6. إرجاع الاستجابة ببيانات المنتج التابعة
                return Ok(new
                {
                    Message = "تمت إضافة المخزون وتحديث الأسعار بنجاح.",
                    product.QuantityInStock,
                    product.CostPrice,
                    product.SellingPrice
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "حدث خطأ أثناء إضافة المخزون.");
            }
        }
    }
}