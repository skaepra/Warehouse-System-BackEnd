using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs.User
{
    public class ToggleUserStatusDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; }
    }
}
