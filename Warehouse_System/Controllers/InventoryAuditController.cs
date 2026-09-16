using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.Data; 
using Online_Store_Backend.DTOs;
using Online_Store_Backend.Models;
using Online_Store_Backend.ResponseDto;
using Online_Store_Backend.Table;

namespace Online_Store_Backend.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize] 
    public class InventoryAuditController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryAuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// (جلب قائمة جميع عمليات الجرد (مع إمكانية التصفية بالحالة 
        [HttpGet("audits")]
        [Authorize(Roles = "Manager, Storekeeper")]
        [ProducesResponseType(typeof(IEnumerable<AuditResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAudits([FromQuery] AuditStatus? status)
        {
            var query = _context.InventoryAudits
                .Include(a => a.Product)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var audits = await query
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new AuditResponseDto
                {
                    Id = a.Id,
                    ProductId = a.ProductId,
                    ProductName = a.Product != null ? a.Product.Name : string.Empty,
                    SystemQuantity = a.SystemQuantity,
                    PhysicalQuantity = a.PhysicalQuantity,
                    Difference = a.Difference,
                    Status = a.Status.ToString(),
                    SubmittedAt = a.SubmittedAt,
                    StorekeeperId = a.StorekeeperId,
                    ManagerId = a.ManagerId
                })
                .ToListAsync();

            return Ok(audits);
        }

        /// جلب تفاصيل طلب جرد محدد برقم المعرف
        [HttpGet("audit/{id}")]
        [Authorize(Roles = "Manager, Storekeeper")]
        [ProducesResponseType(typeof(AuditResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuditById(string id)
        {
            var audit = await _context.InventoryAudits
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
                return NotFound(new { message = "طلب الجرد غير موجود." });

            var response = new AuditResponseDto
            {
                Id = audit.Id,
                ProductId = audit.ProductId,
                ProductName = audit.Product != null ? audit.Product.Name : string.Empty,
                SystemQuantity = audit.SystemQuantity,
                PhysicalQuantity = audit.PhysicalQuantity,
                Difference = audit.Difference,
                Status = audit.Status.ToString(),
                SubmittedAt = audit.SubmittedAt,
                StorekeeperId = audit.StorekeeperId,
                ManagerId = audit.ManagerId
            };

            return Ok(response);
        }

        /// إنشاء طلب جرد جديد 
        [HttpPost("createAudit")]
        [Authorize(Roles = "Storekeeper")]
        [ProducesResponseType(typeof(AuditResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAudit([FromBody] CreateAuditDto dto)
        {
            var storekeeperId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(storekeeperId))
                return Unauthorized();

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new { message = "المنتج غير موجود." });

            var audit = new InventoryAudit
            {
                ProductId = dto.ProductId,
                SystemQuantity = product.QuantityInStock,
                PhysicalQuantity = dto.PhysicalQuantity,
                Status = AuditStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                StorekeeperId = storekeeperId
            };

            _context.InventoryAudits.Add(audit);
            await _context.SaveChangesAsync();

            var response = new AuditResponseDto
            {
                Id = audit.Id,
                ProductId = audit.ProductId,
                ProductName = product.Name,
                SystemQuantity = audit.SystemQuantity,
                PhysicalQuantity = audit.PhysicalQuantity,
                Difference = audit.Difference,
                Status = audit.Status.ToString(),
                SubmittedAt = audit.SubmittedAt,
                StorekeeperId = audit.StorekeeperId
            };

            return CreatedAtAction(nameof(GetAuditById), new { id = audit.Id }, response);
        }



        /// اعتماد طلب الجرد وتعديل مخزون المنتج في النظام  
        [HttpPatch("audit/{id}/approve")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveAudit(string id)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(managerId))
                return Unauthorized();

            var audit = await _context.InventoryAudits
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
                return NotFound(new { message = "طلب الجرد غير موجود." });

            if (audit.Status != AuditStatus.Pending)
                return BadRequest(new { message = "لا يمكن اعتماد طلب جرد تم معالجته مسبقاً." });

            if (audit.Product == null)
                return NotFound(new { message = "المنتج المرتبط بهذا الجرد لم يعد موجوداً." });

            //  تحديث الكمية في جدول المنتجات للكمية الفعلية التي تم جردها
            audit.Product.QuantityInStock = audit.PhysicalQuantity;

            //  تحديث حالة الجرد وتسجيل معرف المدير
            audit.Status = AuditStatus.Approved;
            audit.ManagerId = managerId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم اعتماد الجرد وتحديث كمية المخزون بنجاح." });
        }

        /// رفض طلب الجرد بدون تعديل المخزون  
        [HttpPatch("audit/{id}/reject")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectAudit(string id)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(managerId))
                return Unauthorized();

            var audit = await _context.InventoryAudits.FindAsync(id);
            if (audit == null)
                return NotFound(new { message = "طلب الجرد غير موجود." });

            if (audit.Status != AuditStatus.Pending)
                return BadRequest(new { message = "لا يمكن رفض طلب جرد تم معالجته مسبقاً." });

            audit.Status = AuditStatus.Rejected;
            audit.ManagerId = managerId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم رفض طلب الجرد." });
        }
    }
}