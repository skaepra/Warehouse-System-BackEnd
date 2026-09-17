namespace Warehouse_System_BackEnd.ResponseDto
{
    public class AuditResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int SystemQuantity { get; set; }
        public int PhysicalQuantity { get; set; }
        public int Difference { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string StorekeeperId { get; set; } = string.Empty;
        public string? ManagerId { get; set; }
    }
}
