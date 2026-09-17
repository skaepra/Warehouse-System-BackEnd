using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse_System_BackEnd.Data;
using Warehouse_System_BackEnd.DTOs;
using Warehouse_System_BackEnd.DTOs.Category;
using Warehouse_System_BackEnd.Table;

namespace Warehouse_System_BackEnd.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. استرجاع جميع التصنيفات مع عدد المنتجات في كل تصنيف
        [HttpGet("Categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductsCount = c.Products.Count
                })
                .ToListAsync();

            return Ok(categories);
        }

        // 3. إضافة تصنيف جديد (مسموح للمدير فقط)
        [HttpPost("createCategory")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من عدم تكرار الاسم
            var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.Trim().ToLower());
            if (exists)
            {
                return BadRequest("يوجد تصنيف آخر بنفس هذا الاسم بالفعل.");
            }

            var category = new Category
            {
                Name = dto.Name.Trim()
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var resultDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ProductsCount = 0
            };

            return Ok(resultDto);
        }

        // 4. تعديل تصنيف محدد (مسموح للمدير فقط)
        [HttpPut("updateCategory/{id}")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("التصنيف المراد تعديله غير موجود.");
            }

            // التحقق من عدم تكرار الاسم مع تصنيف آخر
            var exists = await _context.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.Trim().ToLower());
            if (exists)
            {
                return BadRequest("يوجد تصنيف آخر بنفس هذا الاسم.");
            }

            category.Name = dto.Name.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تعديل اسم التصنيف بنجاح." });
        }

        // 5. حذف تصنيف (مسموح للمدير فقط)
        [HttpDelete("deleteCategory/{id}")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound("التصنيف المراد حذفه غير موجود.");
            }

            // منع حذف التصنيف إذا كان مرتبطاً بمنتجات
            if (category.Products.Any())
            {
                return BadRequest("لا يمكن حذف التصنيف لأنه يحتوي على منتجات مرتبطة به. قم بنقل المنتجات أو حذفها أولاً.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف التصنيف بنجاح." });
        }
    }
}