namespace Online_Store_Backend.ResponseDto.Invoice
{
    public class InvoiceSummaryResponseDto
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? IssuedById { get; set; }
    }
}
