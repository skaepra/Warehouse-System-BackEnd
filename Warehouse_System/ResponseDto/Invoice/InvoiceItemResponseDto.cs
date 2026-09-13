namespace Online_Store_Backend.ResponseDto.Invoice
{
    public class InvoiceItemResponseDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitSellingPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitSellingPrice;
    }
}
