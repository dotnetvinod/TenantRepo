using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantEntity = DarshanaTech.Tenant.Domain.Entities.Tenant;

namespace DarshanaTech.Tenant.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.TenantId);

        builder.Property(t => t.SchoolName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.SchoolAddress).HasMaxLength(500);
        builder.Property(t => t.Subdomain).IsRequired().HasMaxLength(63);
        builder.Property(t => t.AdminEmail).IsRequired().HasMaxLength(256);
        builder.Property(t => t.CustomDomain).HasMaxLength(256);
        builder.Property(t => t.BrandLogo).HasMaxLength(500);
        builder.Property(t => t.Timezone).HasMaxLength(100).HasDefaultValue("UTC");
        builder.Property(t => t.Country).HasMaxLength(100).HasDefaultValue("");
        builder.Property(t => t.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(t => t.TenantSettingsJson).HasMaxLength(4000).HasDefaultValue("{}");
        builder.Property(t => t.FeatureFlagsJson).HasMaxLength(2000).HasDefaultValue("{}");

        builder.HasIndex(t => t.Subdomain).IsUnique();
        builder.HasIndex(t => t.Status);

        builder.HasOne(t => t.Plan)
            .WithMany(p => p.Tenants)
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
