namespace Online_Store_Backend.ResponseDto.Product
{
    public class ProductResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public int QuantityInStock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int MinQuantityAlert { get; set; }
    }
}
