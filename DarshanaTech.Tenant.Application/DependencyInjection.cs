using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using DarshanaTech.Tenant.Application.Interfaces;
using DarshanaTech.Tenant.Application.Services;
using DarshanaTech.Tenant.Application.Validators;

namespace DarshanaTech.Tenant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        services.AddValidatorsFromAssemblyContaining<CreateTenantRequestValidator>();
        return services;
    }
}
