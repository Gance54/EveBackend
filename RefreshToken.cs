using System.ComponentModel.DataAnnotations;

namespace EveAuthApi
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = null!;
        
        [Required]
        public int UserId { get; set; }
        
        public User User { get; set; } = null!;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRevoked { get; set; } = false;
    }
} 