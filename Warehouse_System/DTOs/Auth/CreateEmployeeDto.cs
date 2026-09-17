using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Warehouse_System_BackEnd.DTOs.Auth
{
    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("fullName")]

        [Required(ErrorMessage = "اسم الموظف مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة السر يجب أن تكون 6 خانات على الأقل")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "دور الموظف مطلوب")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; // "Storekeeper", "Sales", "Manager"
    }
}
