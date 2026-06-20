using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration: IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.ToTable("Customers");

            entity.HasKey(e => e.CustomerId);

            entity.HasIndex(e => e.UserId)
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.User)
                .WithOne(e => e.Customer)
                .HasForeignKey<Customer>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
