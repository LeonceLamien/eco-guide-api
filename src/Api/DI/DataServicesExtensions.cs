using EcoGuide.Data.Repositories;
using EcoGuide.Data.Seed;

namespace EcoGuide.Api.DI;

public static class DataServicesExtensions
{
    /// <summary>
    /// Register data services, such as repositories, in the DI container.
    /// </summary>
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<DataSeeder>();

        return services;
    }
}