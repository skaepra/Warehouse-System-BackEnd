using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Online_Store_Backend.Data
{
    public class Product
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public string? ImageAlt { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public bool? IsFeatured { get; set; } = false;

        // صور عامة للمنتج (تُستخدم إذا كان المنتج بدون ألوان/متغيرات)
        public List<string> DefaultImages { get; set; } = new();

        // قائمة المتغيرات (الألوان/المقاسات وصورها ومخزونها)
        public List<ProductVariant> Variants { get; set; } = new();
    }
}
