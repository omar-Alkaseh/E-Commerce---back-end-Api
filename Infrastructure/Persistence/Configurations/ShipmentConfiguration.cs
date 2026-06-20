using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> entity)
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__5CAD37ED8BA70A61");

            entity.HasIndex(e => e.OrderId, "IX_Shipments_OrderId");

            entity.HasIndex(e => e.ShippingStatus, "IX_Shipments_ShippingStatus");

            entity.HasIndex(e => e.TrackingNumber, "IX_Shipments_TrackingNumber");

            entity.Property(e => e.CarrierName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ShippingStatus)
                .HasDefaultValue((byte)1)
                .HasComment("1 = Pending\r\n2 = Shipped\r\n3 = InTransit\r\n4 = Delivered\r\n5 = Returned");
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);

            entity.HasOne(d => d.Order).WithOne(p => p.Shipment)
                .HasForeignKey<Shipment>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => s.OrderId)
                .IsUnique();
        }
    }
}
