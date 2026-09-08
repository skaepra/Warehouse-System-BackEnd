using Online_Store_Backend.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs
{
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required")]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public string? ImageAlt { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        public bool? IsFeatured { get; set; } = false;

        // صور عامة للمنتج (تُستخدم إذا كان المنتج بدون ألوان/متغيرات)
        public List<string> DefaultImages { get; set; } = new();
    }
}
