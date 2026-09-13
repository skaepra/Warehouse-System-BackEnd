using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.Order
{
    public class CreateOrderItemDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من 0.")]
        public decimal UnitSellingPrice { get; set; }
    }
}
