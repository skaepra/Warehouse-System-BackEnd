using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Online_Store_Backend.Table
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
