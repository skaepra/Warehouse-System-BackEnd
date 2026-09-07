using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Online_Store_Backend.DTOs
{
    public class CreateUpdateProductVariantDto
    {
        public string? Color { get; set; }        // مثل "Red" أو "#FF0000"
        public string? Size { get; set; }         // مثل "M" أو "42"

        [Required]
        public int StockQuantity { get; set; } = 0; // كمية المخزون الخاص بهذا التنوع

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PriceOverride { get; set; } // سعر خاص بهذا التنوع (اختياري)

        // صور هذا التنوع تحديداً (ستُخزن كـ JSON)
        public List<string> Images { get; set; } = new();
    }
}
