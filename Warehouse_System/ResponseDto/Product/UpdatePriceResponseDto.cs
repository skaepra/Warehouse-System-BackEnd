namespace Warehouse_System_BackEnd.ResponseDto.Product
{
    public class UpdatePriceResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public decimal NewPrice { get; set; }
    }
}
