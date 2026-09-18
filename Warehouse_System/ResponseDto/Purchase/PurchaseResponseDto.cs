namespace Warehouse_System_BackEnd.DTOs.Purchase
{
        public class PurchaseResponseDto
        {
            public string Id { get; set; } = string.Empty;
            public string ProductId { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitCostPrice { get; set; }
            public decimal TotalCost => Quantity * UnitCostPrice;
            public DateTime PurchaseDate { get; set; }
            public string CreatedByUserId { get; set; } = string.Empty;
            public string CreatedByUserName { get; set; } = string.Empty;
        }
}
