using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "اسم التصنيف مطلوب.")]
        [StringLength(100, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز 100 حرف.")]
        public string Name { get; set; } = string.Empty;
    }
}
