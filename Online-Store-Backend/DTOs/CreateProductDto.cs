using System.ComponentModel.DataAnnotations;

namespace Online_Store_Backend.DTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required")]
        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        public string? ImageAlt { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        public bool? IsFeatured { get; set; } = false;

        public List<string> DefaultImages { get; set; } = new();

        public List<CreateUpdateProductVariantDto> Variants { get; set; } = new();
    }

    public class CreateProductResponseDto
    {
        public string Id { get; set; } = string.Empty;
    }
}
