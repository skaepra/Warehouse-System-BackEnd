using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Models;

namespace Warehouse_System_BackEnd.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
