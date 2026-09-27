using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "اسم المنتج مطلوب"), MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; } 

        [Required(ErrorMessage = "التصنيف مطلوب")]
        public string CategoryId { get; set; } = string.Empty;

        public int MinQuantityAlert { get; set; } = 10;
    }
}