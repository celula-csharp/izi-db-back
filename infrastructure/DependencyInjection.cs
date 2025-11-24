using infrastructure.Connections;
using infrastructure.Factory;
using infrastructure.Services.Docker;
using Infrastructure.Services.Docker;
using infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Factory
        services.AddScoped<IDatabaseFactory, DatabaseFactory>();
        
        services.AddScoped<IDockerClientFactory, DockerClientFactory>();
        services.AddScoped<PortFinderService>();
        services.AddScoped<IDockerService, DockerService>();

        return services;
    }
}