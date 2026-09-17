using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.User
{
    public class ManageRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}

