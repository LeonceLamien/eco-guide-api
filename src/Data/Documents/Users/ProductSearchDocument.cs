using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcoGuide.Data.Documents.Users;

/// <summary>
/// Represents a product search entry.
/// </summary>
public class ProductSearchDocument
{
    /// <summary>
    /// Gets or sets the unique identifier of the product search.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the search terms used for the product search.
    /// </summary>
    public string Terms { get; set; } = default!;

    /// <summary>
    /// Gets or sets when the product search was performed.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
