using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcoGuide.Data.Documents.Users;

/// <summary>
/// Represents a product consumption entry.
/// </summary>
public class ProductConsumptionDocument
{
    /// <summary>
    /// Gets or sets the unique identifier of the product consumption.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the id of the product associated with the consumption.
    /// </summary>
    public string ProductId { get; set; } = default!;

    /// <summary>
    /// Gets or sets when the product consumption occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
