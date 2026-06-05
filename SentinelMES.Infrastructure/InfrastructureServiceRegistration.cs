using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SentinelMES.Application.Interfaces;
using SentinelMES.Infrastructure.Persistence;
using SentinelMES.Infrastructure.Repositories;

namespace SentinelMES.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SentinelDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAllowedAssetRepository, AllowedAssetRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();

        return services;
    }
}