using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace Online_Store_Backend.Table
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(150)]
        public string ShopName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string SalespersonId { get; set; }

        [ForeignKey(nameof(SalespersonId))]
        public IdentityUser? Salesperson { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PreparedAt { get; set; }

        // العلاقة مع تفاصيل الطلب
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
