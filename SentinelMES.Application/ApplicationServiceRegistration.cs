using Microsoft.Extensions.DependencyInjection;
using SentinelMES.Application.Interfaces;
using SentinelMES.Application.Services;

namespace SentinelMES.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Siber güvenlik kural motorunu projeye scoped olarak enjekte ediyoruz
        services.AddScoped<ISecurityRuleEngine, SecurityRuleEngine>();

        return services;
    }
}