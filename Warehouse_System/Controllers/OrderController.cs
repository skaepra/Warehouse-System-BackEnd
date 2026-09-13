
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.Data; 
using Online_Store_Backend.DTOs.Order;
using Online_Store_Backend.ResponseDto.Order;
using Online_Store_Backend.Table;
using WarehouseAPI.Models;

namespace Online_Store_Backend.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// جلب كل الطلبات مع إمكانية التصفية بحالة الطلب 
        [HttpGet("orders")]
        [Authorize(Roles = "Sales")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    CustomerName = o.CustomerName,
                    SalespersonId = o.SalespersonId,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    PreparedAt = o.PreparedAt,
                    TotalAmount = o.OrderItems.Sum(i => i.Quantity * i.UnitSellingPrice),
                    Items = o.OrderItems.Select(i => new OrderItemResponseDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product != null ? i.Product.Name : string.Empty,
                        Quantity = i.Quantity,
                        UnitSellingPrice = i.UnitSellingPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        /// Idجلب تفاصيل طلب محدد برقم الـ 
        [HttpGet("order/{id}")]
        [Authorize(Roles = "Sales")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "الطلب غير موجود." });

            var response = new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                SalespersonId = order.SalespersonId,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PreparedAt = order.PreparedAt,
                TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitSellingPrice),
                Items = order.OrderItems.Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : string.Empty,
                    Quantity = i.Quantity,
                    UnitSellingPrice = i.UnitSellingPrice
                }).ToList()
            };

            return Ok(response);
        }

        /// إنشاء طلب جديد بواسطة موظف المبيعات  
        [HttpPost("createOrder")]
        [Authorize(Roles = "Sales")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var salespersonId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(salespersonId))
                return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    CustomerName = dto.CustomerName,
                    SalespersonId = salespersonId,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var itemDto in dto.Items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        return BadRequest(new { message = $"المنتج برقم المعرف '{itemDto.ProductId}' غير موجود." });
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitSellingPrice = itemDto.UnitSellingPrice
                    };

                    order.OrderItems.Add(orderItem);
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { message = "تم إنشاء الطلب بنجاح.", orderId = order.Id });
            }
            //في حال حدوث خطاء مثل انقطاع الاتصال بلنترنيت يتم التراجع عن كافة العمليات حتى لا تُحفظ بيانات ناقصة
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "حدث خطأ أثناء حفظ الطلب." });
            }
        }

        /// تغيير حالة الطلب بواسطة أمين المستودع (Storekeeper) أو المدير (Manager)
        [HttpPatch("order/{id}/status")]
        [Authorize(Roles = "Storekeeper, Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto dto)
        {



            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود." });

                if (order.Status == dto.Status)
                    return BadRequest(new { message = "الطلب يحمل هذه الحالة بالفعل." });

                if (order.Status == OrderStatus.Cancelled)
                    return BadRequest(new { message = "لا يمكن تغيير حالة طلب تم إلغاؤه سابقاً." });

                // 1. الانتقال إلى حالة  (جاهز للتحضير) -> خصم الكميات وتسجيل تاريخ التحضير
                if (dto.Status == OrderStatus.Prepared && order.Status == OrderStatus.Pending)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Product == null)
                        {
                            return BadRequest(new { message = $"المنتج برقم المعرف '{item.ProductId}' غير موجود." });
                        }

                        if (item.Product.QuantityInStock < item.Quantity)
                        {
                            return BadRequest(new { message = $"الكمية المتاحة للمنتج '{item.Product.Name}' في المستودع هي ({item.Product.QuantityInStock}) فقط، ولا تكفي للطلب ({item.Quantity})." });
                        }

                        // خصم الكمية المجهزة من المخزون
                        item.Product.QuantityInStock -= item.Quantity;
                    }

                    order.PreparedAt = DateTime.UtcNow;
                }

                // 2. التسليم (Delivered): إنشاء الفاتورة الرسمية تلقائياً
                if (dto.Status == OrderStatus.Delivered)
                {
                    // التأكد من عدم وجود فاتورة سابقة لنفس الطلب
                    var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.OrderId == order.Id);
                    if (existingInvoice == null)
                    {
                        // حساب إجمالي المبلغ بالفاتورة
                        decimal totalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitSellingPrice);

                        var invoice = new Invoice
                        {
                            OrderId = order.Id,
                            TotalAmount = totalAmount,
                            IssuedAt = DateTime.UtcNow,
                            IssuedById = order.SalespersonId
                        };

                        _context.Invoices.Add(invoice);
                    }
                }

                // 2. إلغاء طلب كان مجهزاً بالفعل -> إعادة الكميات إلى المستودع
                if (dto.Status == OrderStatus.Cancelled && order.Status == OrderStatus.Prepared)
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Product != null)
                        {
                            item.Product.QuantityInStock += item.Quantity;
                        }
                    }
                }

                order.Status = dto.Status;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"تم تغيير حالة الطلب بنجاح إلى '{dto.Status}'." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "حدث خطأ أثناء تعديل حالة الطلب." });
            }
        }
      
    }
}