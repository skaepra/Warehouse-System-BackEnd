using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.ResponseDto.Order
{
    public class OrderResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SalespersonId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PreparedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
