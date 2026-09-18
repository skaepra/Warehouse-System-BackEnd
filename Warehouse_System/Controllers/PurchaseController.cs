using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Warehouse_System_BackEnd.Data;
using Warehouse_System_BackEnd.DTOs.Purchase;
using Warehouse_System_BackEnd.Table;

namespace Warehouse_System_BackEnd.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize] // حماية جميع النقاط بالتوكن
    public class PurchaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// جلب جميع عمليات الشراء
        [HttpGet("Purchases")]
        public async Task<ActionResult<IEnumerable<PurchaseResponseDto>>> GetPurchases()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Product)
                .Include(p => p.CreatedByUser)
                .OrderByDescending(p => p.PurchaseDate)
                .Select(p => new PurchaseResponseDto
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    ProductName = p.Product != null ? p.Product.Name : "غير معروف",
                    Quantity = p.Quantity,
                    UnitCostPrice = p.UnitCostPrice,
                    PurchaseDate = p.PurchaseDate,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedByUserName = p.CreatedByUser != null ? p.CreatedByUser.UserName ?? "غير معروف" : "غير معروف"
                })
                .ToListAsync();

            return Ok(purchases);
        }

        /// جلب عملية شراء محددة بواسطة الـ ID
        [HttpGet("Purchase/{id}")]
        public async Task<ActionResult<PurchaseResponseDto>> GetPurchase(string id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Product)
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound(new { message = "عملية الشراء غير موجودة" });
            }

            var response = new PurchaseResponseDto
            {
                Id = purchase.Id,
                ProductId = purchase.ProductId,
                ProductName = purchase.Product?.Name ?? "غير معروف",
                Quantity = purchase.Quantity,
                UnitCostPrice = purchase.UnitCostPrice,
                PurchaseDate = purchase.PurchaseDate,
                CreatedByUserId = purchase.CreatedByUserId,
                CreatedByUserName = purchase.CreatedByUser?.UserName ?? "غير معروف"
            };

            return Ok(response);
        }

        
    }
}