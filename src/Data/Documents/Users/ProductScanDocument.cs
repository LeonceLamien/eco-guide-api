using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcoGuide.Data.Documents.Users;

/// <summary>
/// Represents a product scan entry.
/// </summary>
public class ProductScanDocument
{
    /// <summary>
    /// Gets or sets the unique identifier of the product scan.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the id of the product associated with the scan.
    /// </summary>
    public string ProductId { get; set; } = default!;

    /// <summary>
    /// Gets or sets when the product scan was performed.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
