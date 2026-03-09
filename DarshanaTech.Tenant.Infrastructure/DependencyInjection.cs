using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Infrastructure.Data;
using DarshanaTech.Tenant.Infrastructure.Repositories;
using DarshanaTech.Tenant.Infrastructure.Services;

namespace DarshanaTech.Tenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITenantRepository>(sp =>
        {
            var context = sp.GetRequiredService<ApplicationDbContext>();
            return new TenantRepository(context);
        });
        services.AddScoped<ITenantCacheService, TenantCacheService>();

        return services;
    }
}
