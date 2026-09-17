using Warehouse_System_BackEnd.Table;

namespace Warehouse_System_BackEnd
{
    public class AddStockResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public Product Product { get; set; } = null!;
    }
}
