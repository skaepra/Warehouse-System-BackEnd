using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.Product
{
    public class UpdatePriceDto
    {
        [Required, Range(0.01, double.MaxValue)]
        public decimal NewSellingPrice { get; set; }
    }
}
