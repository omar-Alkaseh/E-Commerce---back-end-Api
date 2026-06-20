using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> entity)
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3882808B0D");

            entity.HasIndex(e => e.OrderId, "IX_Payments_OrderId");

            entity.HasIndex(e => e.PaymentStatus, "IX_Payments_PaymentStatus");

            entity.HasIndex(e => e.TransactionId, "IX_Payments_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PaymentMethod).HasComment("1 = CashOnDelivery\r\n2 = Card\r\n3 = PayPal");
            entity.Property(e => e.PaymentStatus)
                .HasDefaultValue((byte)1)
                .HasComment("1 = Pending\r\n2 = Paid\r\n3 = Failed\r\n4 = Refunded");
            entity.Property(e => e.TransactionId).HasMaxLength(200);
            entity.Property(e => e.FailureReason)
    .HasMaxLength(500);

            entity.Property(e => e.FailedAt)
                .HasColumnType("datetime2");

            entity.Property(e => e.RefundedAt)
                .HasColumnType("datetime2");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Orders");
        }
    }
}
