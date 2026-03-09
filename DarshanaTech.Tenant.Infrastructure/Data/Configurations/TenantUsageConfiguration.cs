using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Infrastructure.Data.Configurations;

public class TenantUsageConfiguration : IEntityTypeConfiguration<TenantUsage>
{
    public void Configure(EntityTypeBuilder<TenantUsage> builder)
    {
        builder.ToTable("TenantUsage");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.TenantId).IsUnique();

        builder.HasOne(u => u.Tenant)
            .WithOne(t => t.Usage)
            .HasForeignKey<TenantUsage>(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
