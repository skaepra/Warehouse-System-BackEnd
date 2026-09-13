using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.Table
{
    public class Product
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? SKU { get; set; }

        [Required]
        public int QuantityInStock { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        public int MinQuantityAlert { get; set; } = 10;

        public bool IsActive { get; set; } = true;
    }
}
