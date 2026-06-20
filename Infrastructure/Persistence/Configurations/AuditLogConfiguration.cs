using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> entity)
        {
                entity.HasKey(e => e.AuditLogId);

                entity.Property(e => e.Action)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.EntityName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("SYSUTCDATETIME()");
        }
    }
}
