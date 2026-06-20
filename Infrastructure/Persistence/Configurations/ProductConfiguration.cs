using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> entity)
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD9FAE3EAE");

            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.IsActive, "IX_Products_IsActive");

            entity.HasIndex(e => e.IsFeatured, "IX_Products_IsFeatured");

            entity.HasIndex(e => e.Name, "IX_Products_Name");

            entity.HasIndex(e => e.Price, "IX_Products_Price");

            entity.HasIndex(e => e.Sku, "UQ_Products_Sku").IsUnique();

            entity.HasIndex(e => e.Slug, "UQ_Products_Slug").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(p => p.RowVersion).IsRowVersion();

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
                t.HasCheckConstraint("CK_Products_DiscountPrice_Positive", "[DiscountPrice] IS NULL OR [DiscountPrice] >= 0");
                t.HasCheckConstraint("CK_Products_DiscountPrice_LessThanPrice", "[DiscountPrice] IS NULL OR [DiscountPrice] < [Price]");
                t.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            });

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        }
    }
}
