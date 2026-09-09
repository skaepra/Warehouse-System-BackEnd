using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Online_Store_Backend.Table
{
    public class Purchase
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCostPrice { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public IdentityUser? CreatedByUser { get; set; }
    }
}
