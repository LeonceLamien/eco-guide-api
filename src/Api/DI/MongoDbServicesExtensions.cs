using MongoDB.Driver;

namespace EcoGuide.Api.DI;

public static class MongoDbServicesExtensions
{
    /// <summary>
    /// Name of the connection string in appsettings.json
    /// </summary>
    private const string ConnectionStringName = "MongoDb";

    /// <summary>
    /// Register mongoDb services in the DI container.
    /// </summary>
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"{ConnectionStringName} connection string must be set");
        }

        services.AddScoped<IMongoClient>(_ => new MongoClient(connectionString));

        return services;
    }
}