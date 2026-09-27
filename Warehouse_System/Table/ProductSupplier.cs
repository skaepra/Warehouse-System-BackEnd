using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse_System_BackEnd.Table
{
    public class ProductSupplier
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required]
        public string SupplierId { get; set; } = string.Empty;

        [ForeignKey(nameof(SupplierId))]
        public Supplier? Supplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SupplierUnitPrice { get; set; }
    }
}