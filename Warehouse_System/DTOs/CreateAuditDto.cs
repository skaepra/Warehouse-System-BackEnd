using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs
{
    public class CreateAuditDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "الكمية لا يمكن أن تكون بالسالب.")]
        public int PhysicalQuantity { get; set; }
    }
}
