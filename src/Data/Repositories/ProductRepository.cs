using EcoGuide.Data.Constants;
using EcoGuide.Data.Documents.Products;
using EcoGuide.Data.Dtos;
using EcoGuide.Data.Exceptions;
using MongoDB.Driver;

namespace EcoGuide.Data.Repositories;

/// <summary>
/// Handle data operations on <see cref="ProductDto"/>
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product asynchronously by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ProductDto"/> mapped from the product document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the product with the specified ID is not found.</exception>
    Task<ProductDto> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple product asynchronously. [test method]
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A list of <see cref="ProductDto"/> mapped from the product document.</returns>
    Task<List<ProductDto>> ToListAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public class ProductRepository : IProductRepository
{
    private IMongoCollection<ProductDocument> Products { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    /// <param name="client">The MongoDB client used to access the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="client"/> is null.</exception>
    public ProductRepository(IMongoClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var database = client.GetDatabase(Databases.OpenFoodFact.Name);

        Products = database.GetCollection<ProductDocument>(Databases.OpenFoodFact.Collections.Products);
    }

    /// <inheritdoc/>
    public async Task<ProductDto> GetAsync(string id, CancellationToken cancellationToken)
    {
        var product = await Products
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (product == null)
        {
            throw new DocumentNotFoundException<ProductDocument>(id);
        }

        return ProductDto.FromDocument(product);
    }

    public async Task<List<ProductDto>> ToListAsync(CancellationToken cancellationToken)
    {
        var products = await Products
            .Find(FilterDefinition<ProductDocument>.Empty)
            .Limit(5)
            .ToListAsync(cancellationToken);
        return products.Select(ProductDto.FromDocument).ToList();
    }
}

