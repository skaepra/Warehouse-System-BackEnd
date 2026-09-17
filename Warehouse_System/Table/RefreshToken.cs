using Microsoft.AspNetCore.Identity;

namespace Warehouse_System_BackEnd.Table
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;
        public DateTime JwtId { get; set; }
        public DateTime IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
