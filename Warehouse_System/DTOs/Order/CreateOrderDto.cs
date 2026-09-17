using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Order
{
    public class CreateOrderDto
    {

        [Required]
        [MaxLength(150)]
        public string ShopName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "يجب إضافة منتج واحد على الأقل للطلب.")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
