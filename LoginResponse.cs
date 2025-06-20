namespace EveAuthApi
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public int AccessTokenExpiresIn { get; set; } // 1 hour
        public int RefreshTokenExpiresIn { get; set; } // 1 month
        public string TokenType { get; set; } = "";
        public DateTime IssuedAt { get; set; }
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public bool IsSubscribed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 