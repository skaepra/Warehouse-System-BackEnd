using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Models;

namespace Online_Store_Backend.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
