using System.ComponentModel.DataAnnotations;

namespace EveAuthApi
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        public string PasswordHash { get; set; } = null!;
        
        public bool IsSubscribed { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<AccessToken> AccessTokens { get; set; } = new List<AccessToken>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
