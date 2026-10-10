using EcoGuide.Data.Documents.Products;
using EcoGuide.Data.Dtos.Enums;

namespace EcoGuide.Data.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for product information.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the nutriscore of the product.
    /// </summary>
    public NutriScore Nutriscore { get; set; }

    /// <summary>
    /// Converts a <see cref="ProductDocument"/> to a ProductDto object.
    /// </summary>
    /// <param name="document">The product document to convert.</param>
    /// <returns>A new instance of ProductDto populated with data from the provided document.</returns>
    public static ProductDto FromDocument(ProductDocument document)
    {
        var nutriscore = NutriScore.Unknown;

        if (Enum.TryParse(document.Nutriscore, ignoreCase: true, out NutriScore parsedNutriscore))
        {
            nutriscore = parsedNutriscore;
        }

        return new ProductDto
        {
            Id = document.Id,
            Name = document.Name,
            Nutriscore = nutriscore
        };
    }
}
