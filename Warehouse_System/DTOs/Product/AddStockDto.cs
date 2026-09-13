using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.Product
{
    public class AddStockDto
    {
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal UnitCostPrice { get; set; }
    }
}
