using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcoGuide.Data.Constants;
using EcoGuide.Data.Documents.Products;
using EcoGuide.Data.Dtos;
using EcoGuide.Data.Dtos.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace EcoGuide.Api.Integration.Tests;

public class ProductsTests : IClassFixture<MongoDbFixture>, IDisposable
{
    private readonly MongoDbFixture _fixture;
    private readonly HttpClient _client;

    public ProductsTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
        var appFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IMongoClient>();
                    services.AddScoped<IMongoClient>(
                        (_) => _fixture.Client);
                });
            });
        _client = appFactory.CreateClient();
    }

    [Fact]
    public async Task GetAsync_ProductExists_ShouldReturnProduct()
    {
        // Arrange
        var database = _fixture.Client.GetDatabase(Databases.OpenFoodFact.Name);
        var collection = database.GetCollection<ProductDocument>(Databases.OpenFoodFact.Collections.Products);

        await collection.InsertOneAsync(new ProductDocument
        {
            Id = "1111", Name = "Test product", Nutriscore = "a"
        });

        // Act
        var res = await _client.GetAsync("api/products/1111");
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadAsStringAsync();

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        var product = JsonSerializer.Deserialize<ProductDto>(content, jsonSerializerOptions);

        // Assert

        var expectedProduct = new ProductDto { Id = "1111", Name = "Test product", Nutriscore = NutriScore.A };

        product.Should().NotBeNull();
        product.Should().BeEquivalentTo(expectedProduct);
    }

    [Fact]
    public async Task GetAsync_ProductDoesNotExists_ShouldReturn404()
    {
        // Arrange
        var database = _fixture.Client.GetDatabase(Databases.OpenFoodFact.Name);
        var collection = database.GetCollection<ProductDocument>(Databases.OpenFoodFact.Collections.Products);

        await collection.InsertOneAsync(new ProductDocument
        {
            Id = "2222",
            Name = "Test product",
            Nutriscore = "a"
        });

        // Act
        var res = await _client.GetAsync("api/products/1111");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await res.Content.ReadAsStringAsync();
        content.Should().Be("ProductDocument with id 1111 does not exists.");
    }

    public void Dispose()
    {
        var database = _fixture.Client.GetDatabase(Databases.OpenFoodFact.Name);
        var collection = database.GetCollection<ProductDocument>(Databases.OpenFoodFact.Collections.Products);
        collection.DeleteManyAsync(_ => true).Wait();
    }
}