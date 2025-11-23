using application.Queries;
using application.Queries.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Query Executor
        services.AddScoped<IDatabaseQueryExecutor, DatabaseQueryExecutor>();
        
        return services;
    }
}