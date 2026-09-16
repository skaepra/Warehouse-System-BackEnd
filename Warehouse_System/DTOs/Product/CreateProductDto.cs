using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.Product
{
    public class CreateProductDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; }

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [Required, Range(1, int.MaxValue)]
        public int InitialQuantity { get; set; } 

        [Required, Range(0.01, double.MaxValue)]
        public decimal UnitCostPrice { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal SellingPrice { get; set; }

        public int MinQuantityAlert { get; set; } = 10;
    }

  

   
}
