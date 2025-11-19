using application.Docker.Interfaces;
using application.Docker.Services;
using infrastructure.Docker.Configuration;
using infrastructure.Docker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Docker.DotNet;

namespace infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Docker configuration binding
        services.Configure<IOptions<DockerConfiguration>>(
            configuration.GetSection(DockerConfiguration.SectionName));
        
        // Application services
        services.AddScoped<IDockerService, DockerService>();
        services.AddScoped<IPortFinderService, PortFinderService>();

        // Register DockerClient as singleton
        services.AddSingleton<DockerClient>(provider =>
        {
            var cfg = provider.GetRequiredService<IOptions<DockerConfiguration>>().Value;
            var clientConfig = new DockerClientConfiguration(new Uri(cfg.Host));
            return clientConfig.CreateClient();
        });

        return services;
    }
}