using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(l => l.LogId);

        builder.Property(l => l.Action).IsRequired().HasMaxLength(100);
        builder.Property(l => l.PerformedBy).IsRequired().HasMaxLength(256);
        builder.Property(l => l.Metadata).HasMaxLength(4000);

        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.Timestamp);
    }
}
