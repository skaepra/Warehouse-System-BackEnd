using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.User
{
    public class ToggleUserStatusDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; }
    }
}
