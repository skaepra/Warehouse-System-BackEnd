using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.ResponseDto.Suppliers;
using Warehouse_System_BackEnd.Data;
using Warehouse_System_BackEnd.DTOs;
using Warehouse_System_BackEnd.DTOs.Supplier;
using Warehouse_System_BackEnd.Table;

namespace Warehouse_System_BackEnd.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize] 
    public class SuppliersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. جلب قائمة الموردين
        [HttpGet("suppliers")]
        [Authorize(Roles = "SalesRepresentative")]
        [ProducesResponseType(typeof(IEnumerable<SupplierResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuppliers([FromQuery] bool activeOnly = false)
        {
            var query = _context.Suppliers.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }

            var suppliers = await query
                .Select(s => new SupplierResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Phone = s.Phone,
                    IsActive = s.IsActive
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(suppliers);
        }

        // 2. عرض الموردين المرتبطين بمنتج معين
        [HttpGet("productSupplier/{productId}")]
        [Authorize(Roles = "SalesRepresentative , Manager")]
        [ProducesResponseType(typeof(IEnumerable<SupplierResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSuppliersByProductId(string productId)
        {
            // التحقق من وجود المنتج أولاً
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                return NotFound("المنتج غير موجود.");

            // جلب الموردين المرتبطين بهذا المنتج عبر جدول الكسر ProductSupplier
            var suppliers = await _context.ProductSuppliers
                .Where(ps => ps.ProductId == productId && ps.Supplier!.IsActive)
                .Select(ps => new SupplierResponseDto
                {
                    Id = ps.Supplier!.Id,
                    Name = ps.Supplier.Name,
                    Phone = ps.Supplier.Phone,
                    IsActive = ps.Supplier.IsActive, 
                    SupplierUnitPrice = ps.SupplierUnitPrice
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(suppliers);
        }

       

        // 3. إضافة مورد جديد 
        [HttpPost("createSupplier")]
        [Authorize(Roles = "SalesRepresentative")]
        [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            // التحقق من عدم تكرار اسم المورد
            var trimmedName = dto.Name.Trim().ToLower();
            var exists = await _context.Suppliers
                .AnyAsync(s => s.Name.Trim().ToLower() == trimmedName);

            if (exists)
                return BadRequest(new { message = "يوجد مورد مسجل بهذا الاسم بالفعل." });

            var supplier = new Supplier
            {
                Name = dto.Name.Trim(),
                Phone = dto.Phone?.Trim(),
                IsActive = true
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var response = new SupplierResponseDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                IsActive = supplier.IsActive
            };

            // استخدام اسم المسار المسمى "GetSupplierById" لتوليد الرابط الصحيح في الهيدر (Location Header)
            return CreatedAtRoute("GetSupplierById", new { id = supplier.Id }, response);
        }

        // 4. تعديل بيانات مورد موجود 
        [HttpPut("updateSupplier/{id}")]
        [Authorize(Roles = "SalesRepresentative")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(string id, [FromBody] CreateSupplierDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound("المورد غير موجود.");

            supplier.Name = dto.Name.Trim();
            supplier.Phone = dto.Phone?.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث بيانات المورد بنجاح." });
        }

        // 5. تعطيل/تفعيل مورد 
        [HttpPatch("supplier/{id}/toggle-status")]
        [Authorize(Roles = "SalesRepresentative")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleSupplierStatus(string id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound("المورد غير موجود.");

            supplier.IsActive = !supplier.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تغيير حالة المورد بنجاح.", isActive = supplier.IsActive });
        }


        // 6. ربط منتج موجود مع مورد محدد مع تحديد سعر المنتج
        [HttpPost("supplier/assign-product")]
        [Authorize(Roles = "SalesRepresentative")]    
        public async Task<IActionResult> AssignProductToSupplier([FromBody] AssignProductToSupplierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من وجود المورد والمنتج
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId && s.IsActive);
            if (!supplierExists)
                return NotFound(new { message = "المورد غير موجود أو غير نشط." });

            var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId && p.IsActive);
            if (!productExists)
                return NotFound(new { message = "المنتج غير موجود أو غير نشط." });

            // التحقق مما إذا كان الربط موجوداً مسبقاً
            var existingRelation = await _context.ProductSuppliers
                .FirstOrDefaultAsync(ps => ps.ProductId == dto.ProductId && ps.SupplierId == dto.SupplierId);

            if (existingRelation != null)
            {
                // إذا كان الربط موجوداً، نقوم بتحديث السعر
                existingRelation.SupplierUnitPrice = dto.SupplierUnitPrice;
                _context.ProductSuppliers.Update(existingRelation);
            }
            else
            {
                // إضافه سجل ربط جديد
                var productSupplier = new ProductSupplier
                {
                    ProductId = dto.ProductId,
                    SupplierId = dto.SupplierId,
                    SupplierUnitPrice = dto.SupplierUnitPrice
                };
                await _context.ProductSuppliers.AddAsync(productSupplier);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إسناد المنتج للمورد وتحديث السعر بنجاح." });
        }
    }
}