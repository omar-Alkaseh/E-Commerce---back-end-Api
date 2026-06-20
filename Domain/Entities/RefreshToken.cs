namespace Domain.Entities;

public partial class RefreshToken
{
    public int RefreshTokenId { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string? JwtId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public bool IsUsed { get; set; }

    public User User { get; set; } = null!;
}
