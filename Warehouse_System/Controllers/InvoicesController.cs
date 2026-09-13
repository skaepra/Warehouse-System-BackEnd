using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.Data;
using Online_Store_Backend.ResponseDto.Invoice;
using Online_Store_Backend.Table;

namespace Online_Store_Backend.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// جلب جميع الفواتير الصادرة
        [HttpGet("Invoices")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceSummaryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Order)
                .OrderByDescending(i => i.IssuedAt)
                .Select(i => new InvoiceSummaryResponseDto
                {
                    InvoiceId = i.Id,
                    OrderId = i.OrderId,
                    CustomerName = i.Order != null ? i.Order.CustomerName : string.Empty,
                    TotalAmount = i.TotalAmount,
                    IssuedAt = i.IssuedAt,
                    IssuedById = i.IssuedById
                })
                .ToListAsync();

            return Ok(invoices);
        }

        /// Idجلب تفاصيل فاتورة واحدة برقم الـ 
        [HttpGet("Invoices/{id}/details")]
        [ProducesResponseType(typeof(InvoiceDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoiceById(string id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o!.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "الفاتورة غير موجودة." });

            var response = new InvoiceDetailsResponseDto
            {
                InvoiceId = invoice.Id,
                OrderId = invoice.OrderId,
                CustomerName = invoice.Order?.CustomerName ?? string.Empty,
                TotalAmount = invoice.TotalAmount,
                IssuedAt = invoice.IssuedAt,
                IssuedById = invoice.IssuedById,
                Items = invoice.Order?.OrderItems.Select(oi => new InvoiceItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitSellingPrice = oi.UnitSellingPrice
                }).ToList() ?? new List<InvoiceItemResponseDto>()
            };

            return Ok(response);
        }
    }
}