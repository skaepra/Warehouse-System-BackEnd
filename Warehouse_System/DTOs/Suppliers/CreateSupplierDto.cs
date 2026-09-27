using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.DTOs.Supplier
{
    public class CreateSupplierDto
    {
        [Required(ErrorMessage = "اسم المورد مطلوب.")]
        public string Name { get; set; } = string.Empty;

        public string? ContactPerson { get; set; }

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح.")]
        public string? Phone { get; set; }

    }
}