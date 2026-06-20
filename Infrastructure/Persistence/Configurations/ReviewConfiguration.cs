using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> entity)
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE60AE643B");

            entity.HasIndex(e => e.IsApproved, "IX_Reviews_IsApproved");

            entity.HasIndex(e => e.ProductId, "IX_Reviews_ProductId");

            entity.HasIndex(e => e.Rating, "IX_Reviews_Rating");

            entity.HasIndex(e => new { e.CustomerId, e.ProductId }, "UQ_Reviews_CustomerId_ProductId").IsUnique();

            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Customers");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Reviews_Products");
        }
    }
}
