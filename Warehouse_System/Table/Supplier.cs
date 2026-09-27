
using System.ComponentModel.DataAnnotations;

namespace Warehouse_System_BackEnd.Table
{
    public class Supplier
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // العلاقة متعدد إلى متعدد عبر جدول الكسر
        public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    }
}