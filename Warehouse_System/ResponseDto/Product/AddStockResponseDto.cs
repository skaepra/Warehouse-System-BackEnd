using Online_Store_Backend.Table;

namespace Online_Store_Backend
{
    public class AddStockResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public Product Product { get; set; } = null!;
    }
}
