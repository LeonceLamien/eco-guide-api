﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

using EcoGuide.Data.Seed;
using EcoGuide.Api.DI;

namespace EcoGuide.Api;

/// <summary>
/// Represents the startup class for configuring services and the application's request pipeline.
/// </summary>
/// <param name="configuration">An instance of <see cref="IConfiguration"/> used to access configuration settings.</param>
public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // <summary>
    /// Configures the services for the application.
    /// </summary>
    /// <param name="services">The collection of service descriptors.</param>
    /// <remarks>
    /// This method is used to add services to the dependency injection container.
    /// It configures HTTP client services, hosted services, controllers with JSON options,
    /// MongoDB services, data services, CORS policies, MVC services, and JWT authentication.
    /// </remarks>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddHostedService<CronDbService>();
        services.AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddMongoDb(Configuration);

        services.AddDataServices();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                builder => builder.WithOrigins("http://localhost:55812")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                );
        });

        services.AddMvc();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{Configuration["Auth0:Domain"]}/";
                options.Audience = Configuration["Auth0:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = new[] { "https://dev-czb66bs1etiil5pq.us.auth0.com/api/v2/users", "jdmUA", "https://dev-czb66bs1etiil5pq.us.auth0.com/api/v2/" }, // List of allowed audiences
                };
            });
    }

    /// <summary>
    /// Configures the HTTP request pipeline.
    /// </summary>
    /// <param name="app">An instance of <see cref="IApplicationBuilder"/> to configure the application's request pipeline.</param>
    /// <param name="env">An instance of <see cref="IWebHostEnvironment"/> that provides information about the web hosting environment.</param>
    /// <param name="seedData">An instance of <see cref="DataSeeder"/> used to initialize the database with seed data.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataSeeder seedData)
    {
        if (env.IsDevelopment())
        {
            seedData.InitializeEcoGuideDb();
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("AllowSpecificOrigin");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });
    }
}