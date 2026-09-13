using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.User
{
    public class ManageRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}

