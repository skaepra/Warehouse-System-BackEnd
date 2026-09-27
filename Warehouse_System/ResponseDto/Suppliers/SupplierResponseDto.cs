namespace Online_Store_Backend.ResponseDto.Suppliers
{
    public class SupplierResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public decimal SupplierUnitPrice { get; set; }
    }
}