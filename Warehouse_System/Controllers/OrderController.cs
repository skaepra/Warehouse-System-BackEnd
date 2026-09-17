
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse_System_BackEnd.Data; 
using Warehouse_System_BackEnd.DTOs.Order;
using Warehouse_System_BackEnd.ResponseDto.Order;
using Warehouse_System_BackEnd.Table;
using WarehouseAPI.Models;

namespace Warehouse_System_BackEnd.Controllers
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

        [HttpGet("orders")]
        [Authorize(Roles = "Sales, Storekeeper, Manager")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders([FromQuery] string? status)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // تحويل النص الممرر إلى قيمة Enum إن وجد
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(o => o.Status == parsedStatus);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    ShopName = o.ShopName,
                    Address = o.Address,
                    SalespersonId = o.SalespersonId,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    PreparedAt = o.PreparedAt,
                    TotalAmount = o.OrderItems.Sum(i => i.Quantity * i.UnitSellingPrice),
                    Items = o.OrderItems.Select(i => new OrderItemResponseDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name ?? string.Empty,
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
                ShopName = order.ShopName,
                Address = order.Address,
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

        /// جلب كافة الطلبات الخاصة بمندوب المبيعات الحالي
        [HttpGet("myOrders")]
        [Authorize(Roles = "Sales")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrders()
        {
            // استخراج معرّف المندوب من الـ Token
            var salespersonId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(salespersonId))
                return Unauthorized();

            // جلب طلبات المندوب مع تضمين المنتجات لحساب التفاصيل والأسماء
            var orders = await _context.Orders
                .Where(o => o.SalespersonId == salespersonId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt) // ترتيب الطلبات من الأحدث للأقدم
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    ShopName = o.ShopName,
                    Address = o.Address,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitSellingPrice),
                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                        Quantity = oi.Quantity,
                        UnitSellingPrice = oi.UnitSellingPrice
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(orders);
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
                    ShopName = dto.ShopName,
                    Address = dto.Address,
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

                    // 1. التحقق من التوفر فوراً عند إنشاء المندوب للطلب
                    if (product.QuantityInStock < itemDto.Quantity)
                    {
                        return BadRequest(new { message = $"الكمية المتاحة للمنتج '{product.Name}' هي ({product.QuantityInStock}) فقط، ولا تكفي للطلب ({itemDto.Quantity})." });
                    }

                    // 2. خصم الكمية فوراً لحجزها للمندوب وتفادي البيع الزائد (Overselling)
                    product.QuantityInStock -= itemDto.Quantity;

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

                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { message = "تم إنشاء الطلب وخصم الكمية بنجاح.", orderId = order.Id });
            }//في حال حدوث خطاء مثل انقطاع الاتصال بلنترنيت يتم التراجع عن كافة العمليات حتى لا تُحفظ بيانات ناقصة
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
            // 1. التحقق من صحة النص القادم وتحويله إلى Enum
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            {
                return BadRequest(new { message = $"حالة الطلب غير صالحة: '{dto.Status}'." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود." });

                if (order.Status == newStatus)
                    return BadRequest(new { message = "الطلب يحمل هذه الحالة بالفعل." });

                if (order.Status == OrderStatus.Cancelled)
                    return BadRequest(new { message = "لا يمكن تغيير حالة طلب تم إلغاؤه سابقاً." });

                // 2. الانتقال إلى حالة (جاهز للتحضير)
                if (newStatus == OrderStatus.Prepared && order.Status == OrderStatus.Pending)
                {
                    order.PreparedAt = DateTime.UtcNow;
                }

                // 3. التسليم (Delivered): إنشاء الفاتورة الرسمية تلقائياً
                if (newStatus == OrderStatus.Delivered)
                {
                    var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.OrderId == order.Id);
                    if (existingInvoice == null)
                    {
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

                // 4. إلغاء طلب كان مجهزاً أو قيد الانتظار -> إعادة الكميات إلى المستودع
                if (newStatus == OrderStatus.Cancelled && (order.Status == OrderStatus.Prepared || order.Status == OrderStatus.Pending))
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Product != null)
                        {
                            item.Product.QuantityInStock += item.Quantity;
                        }
                    }
                }

                // إسناد الحالة جديدة المقبولة
                order.Status = newStatus;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"تم تغيير حالة الطلب بنجاح إلى '{newStatus}'." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "حدث خطأ أثناء تعديل حالة الطلب." });
            }
        }          

        /// إلغاء الطلب بواسطة المندوب بشرط أن يكون قيد الانتظار (Pending)
        [HttpPut("cancelOrder/{id}")]
        [Authorize(Roles = "Sales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(string id)
        {
            var salespersonId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(salespersonId))
                return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.SalespersonId == salespersonId);

                if (order == null)
                {
                    return NotFound(new { message = "الطلب غير موجود أو لا تملك صلاحية الوصول إليه." });
                }

                if (order.Status != OrderStatus.Pending)
                {
                    return BadRequest(new { message = "لا يمكن إلغاء الطلب لأنه تم تجهيزه أو الموافقة عليه بالفعل." });
                }

                // إرجاع الكميات المخصومة للمخزون
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.QuantityInStock += item.Quantity;
                    }
                }

                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "تم إلغاء الطلب وإعادة الكميات للمخزون بنجاح.", orderId = order.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "حدث خطأ أثناء إلغاء الطلب." });
            }
        }

    }
}