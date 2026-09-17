using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "اسم التصنيف مطلوب.")]
        [StringLength(100, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز 100 حرف.")]
        public string Name { get; set; } = string.Empty;
    }
}
