namespace Online_Store_Backend.ResponseDto
{
    public class AuthResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
