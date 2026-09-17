using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Product
{
    public class UpdatePriceDto
    {
        [Required, Range(0.01, double.MaxValue)]
        public decimal NewSellingPrice { get; set; }
    }
}
