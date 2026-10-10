using MongoDB.Bson.Serialization.Attributes;

namespace EcoGuide.Data.Documents.Products;

/// <summary>
/// Represents a product in the open food fact database.
/// </summary>
[BsonIgnoreExtraElements]
public class ProductDocument
{
    /// <summary>
    /// Gets or sets the unique identifier of the product document.
    /// </summary>
    [BsonId]
    [BsonElement("_id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [BsonElement("product_name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the nutriscore of the product.
    /// </summary>
    [BsonElement("nutriscore_grade")]
    public string? Nutriscore { get; set; }
}