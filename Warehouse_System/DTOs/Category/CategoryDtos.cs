namespace Online_Store_Backend.DTOs.Category
{
    public class CategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int ProductsCount { get; set; } // عدد المنتجات المرتبطة بهذا التصنيف
    }
}
