namespace Online_Store_Backend.ResponseDto.Order
{
    public class OrderItemResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitSellingPrice { get; set; }
        public decimal TotalItemPrice => Quantity * UnitSellingPrice;
    }
}
