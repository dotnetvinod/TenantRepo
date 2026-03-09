using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DarshanaTech.Tenant.Domain.Entities;

namespace DarshanaTech.Tenant.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.PlanId);

        builder.Property(p => p.PlanId).ValueGeneratedOnAdd();

        builder.Property(p => p.PlanName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.FeaturesJson).HasMaxLength(4000).HasDefaultValue("[]");
        builder.Property(p => p.Price).HasPrecision(18, 2);
    }
}
