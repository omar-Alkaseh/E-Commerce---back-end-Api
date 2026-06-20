using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    internal class ProductImageConfiguration :  IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> entity)
        {
            entity.HasKey(e => e.ProductImageId).HasName("PK__ProductI__07B2B1B8961E3380");

            entity.HasIndex(e => e.IsMain, "IX_ProductImages_IsMain");

            entity.HasIndex(e => e.ProductId, "IX_ProductImages_ProductId");

            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasIndex(e => new { e.ProductId, e.SortOrder })
                .HasDatabaseName("IX_ProductImages_ProductId_SortOrder");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductImages_Products");
        }
    }
}
