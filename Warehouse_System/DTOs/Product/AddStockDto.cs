using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Product
{
    public class AddStockDto
    {
        public int Quantity { get; set; }
        public decimal UnitCostPrice { get; set; }
        public string SupplierId { get; set; } = string.Empty;
    }
}
