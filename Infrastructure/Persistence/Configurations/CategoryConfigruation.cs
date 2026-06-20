using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CategoryConfigruation : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B55A79060");

            entity.HasIndex(e => e.IsActive, "IX_Categories_IsActive");

            entity.HasIndex(e => e.ParentCategoryId, "IX_Categories_ParentCategoryId");

            entity.HasIndex(e => e.Slug, "UQ_Categories_Slug").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(150);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_ParentCategory");
        }
    }
}
