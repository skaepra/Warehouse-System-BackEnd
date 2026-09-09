using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        public string Password { get; set; } = string.Empty;
    }
}
