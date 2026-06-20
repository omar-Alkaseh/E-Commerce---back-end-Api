using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration: IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> entity)
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFE8FCF03E");

            entity.HasIndex(e => e.CreatedAt, "IX_Orders_CreatedAt");

            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");

            entity.HasIndex(e => e.OrderStatus, "IX_Orders_OrderStatus");

            entity.HasIndex(e => e.OrderNumber, "UQ_Orders_OrderNumber").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.OrderStatus)
                .HasDefaultValue((byte)1)
                .HasComment("1 = Pending\r\n2 = Processing\r\n3 = Shipped\r\n4 = Delivered\r\n5 = Cancelled");
            entity.Property(e => e.ShippingAddressLine).HasMaxLength(300);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Customers");
        }
    }
}
