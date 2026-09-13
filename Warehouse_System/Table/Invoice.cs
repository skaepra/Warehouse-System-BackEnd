using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Models;

namespace Online_Store_Backend.Table
{
    public class Invoice
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public string? IssuedById { get; set; }

        [ForeignKey(nameof(IssuedById))]
        public IdentityUser? IssuedBy { get; set; }
    }
}
