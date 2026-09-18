using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Purchase
{
    public class CreatePurchaseDto
    {
        [Required(ErrorMessage = "معرف المنتج مطلوب")]
        public string ProductId { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "سعر التكلفة مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر التكلفة يجب أن يكون أكبر من 0")]
        public decimal UnitCostPrice { get; set; }
    }
}