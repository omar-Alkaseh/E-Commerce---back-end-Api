using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> entity)
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A873B07BD");

            entity.HasIndex(e => e.RoleName, "UQ_Roles_RoleName").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        }
    }
}
