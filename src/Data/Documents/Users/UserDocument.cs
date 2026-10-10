using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

using EcoGuide.Data.Dtos.Enums;

namespace EcoGuide.Data.Documents.Users;

/// <summary>
/// An EcoGuide user.
/// </summary>
public class UserDocument
{
    /// <summary>
    /// Gets or sets the unique identifier of the user document.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; } = default!;
    /// <summary>
    /// Gets or sets the unique identifier of the user document.
    /// </summary>
    public string userId { get; set; } = default!;
    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public string first_name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public string last_name { get; set; } = default!;
    
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string email { get; set; } = default!;
    /// <summary>
    /// Gets or sets the user's preferred minimum Nutri-Score.
    /// </summary>
    public NutriScore min_nutri_score { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user's preferred minimum Eco-Score.
    /// </summary>
    public EcoScore min_eco_score { get; set; } = default!;
    /// <summary>
    /// Gets or sets the country of residence of the user.
    /// </summary>
    
    public string country { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of cuisine preferred by the user.
    /// </summary>
    public string cuisine_type { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of allergens for the user.
    /// </summary>
    public List<string> allergens { get; set; } = [];

    /// <summary>
    /// Gets or sets the email sending status for the user.
    /// </summary>
    public bool is_sending_email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of product searches performed by the user.
    /// </summary>
    public List<ProductSearchDocument> Searches { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of product scans performed by the user.
    /// </summary>
    public List<ProductScanDocument> Scans { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of product consumptions by the user.
    /// </summary>
    public List<ProductConsumptionDocument> Consumptions { get; set; } = [];
}