namespace EveAuthApi
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserInfo User { get; set; } = null!;
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public bool IsSubscribed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 