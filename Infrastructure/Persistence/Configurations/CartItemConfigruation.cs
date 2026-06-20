using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CartItemConfigruation : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> entity)
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0AD3B25CF9");

            entity.HasIndex(e => e.ProductId, "IX_CartItems_ProductId");

            entity.HasIndex(e => new { e.CartId, e.ProductId }, "UQ_CartItems_CartId_ProductId").IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_CartItems_Quantity",
                "[Quantity] > 0"));

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_CartItems_Carts");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CartItems_Products");
        }
    }
}
