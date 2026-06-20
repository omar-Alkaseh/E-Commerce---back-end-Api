using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> entity)
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7B71CFF4A12");

            entity.HasIndex(e => e.CustomerId, "IX_Carts_CustomerId")
                .IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Customer).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.CustomerId)
                .HasConstraintName("FK_Carts_Customers");
        }
    }
}
