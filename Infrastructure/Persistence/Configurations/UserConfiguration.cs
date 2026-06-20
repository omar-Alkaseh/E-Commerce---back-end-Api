using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsEmailConfirmed)
                .HasDefaultValue(false);

            entity.Property(e => e.FailedLoginAttempts)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    right => right.HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_UserRoles_Roles")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UserRoles_Users")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("UserRoles");
                        join.HasKey("UserId", "RoleId");
                    });
        }
    }
}
