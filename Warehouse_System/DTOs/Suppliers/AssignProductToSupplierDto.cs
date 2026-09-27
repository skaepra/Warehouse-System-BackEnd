using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs
{
    public class AssignProductToSupplierDto
    {
        [Required(ErrorMessage = "معرف المورد مطلوب")]
        public string SupplierId { get; set; } = string.Empty;

        [Required(ErrorMessage = "معرف المنتج مطلوب")]
        public string ProductId { get; set; } = string.Empty;

        [Required(ErrorMessage = "سعر المورد مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
        public decimal SupplierUnitPrice { get; set; }
    }
}