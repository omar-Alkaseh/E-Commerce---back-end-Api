using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> entity)
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E39E1A9F3FA");

            entity.HasIndex(e => e.UserId);

            entity.HasIndex(e => e.ExpiresAt, "IX_RefreshTokens_ExpiresAt");

            entity.HasIndex(e => e.TokenHash, "UQ_RefreshTokens_TokenHash").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedByIp).HasMaxLength(50);
            entity.Property(e => e.JwtId).HasMaxLength(200);
            entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(500);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.TokenHash).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
