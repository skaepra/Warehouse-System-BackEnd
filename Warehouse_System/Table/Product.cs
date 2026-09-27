using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse_System_BackEnd.Table
{
    public class Product
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; } 

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public int QuantityInStock { get; set; } = 0; 

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } = 0;

        public int MinQuantityAlert { get; set; } = 10;

        public bool IsActive { get; set; } = true;

        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    }
}