using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Online_Store_Backend.Models;

namespace Online_Store_Backend.Table
{
    public class InventoryAudit
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required]
        public int SystemQuantity { get; set; }

        [Required]
        public int PhysicalQuantity { get; set; }

        // الفارق بين الحقيقي والمسجل
        public int Difference => PhysicalQuantity - SystemQuantity;

        [Required]
        public AuditStatus Status { get; set; } = AuditStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string StorekeeperId { get; set; }

        [ForeignKey(nameof(StorekeeperId))]
        public IdentityUser? Storekeeper { get; set; }

        public string? ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public IdentityUser? Manager { get; set; }
    }
}
