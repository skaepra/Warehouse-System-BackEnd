using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Warehouse_System_BackEnd.Table
{
    public class Category
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore] // لمنع الـ Circular Reference أثناء الـ Serialization
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
