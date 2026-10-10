using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

using EcoGuide.Data.Documents.Users;
using EcoGuide.Data.Dtos.Enums;

namespace EcoGuide.Data.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for product information.
/// </summary>
public class UserDto
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
    public List<ProductConsumptionDocument> Consumptions { get; set; } = new();

    /// <summary>
    /// Converts a <see cref="UserDocument"/> to a <see cref="UserDto"/>.
    /// </summary>
    /// <param name="document">The <see cref="UserDocument"/> to convert.</param>
    /// <returns>A <see cref="UserDto"/> that represents the given <see cref="UserDocument"/>.</returns>
    public static UserDto FromDocument(UserDocument document){
        return new UserDto{
            _id = document._id,
            userId = document.userId,
            email = document.email,
            first_name = document.first_name,
            last_name = document.last_name,
            min_nutri_score = document.min_nutri_score,
            min_eco_score = document.min_eco_score,
            country = document.country,
            cuisine_type = document.cuisine_type,
            allergens = document.allergens,
            is_sending_email = document.is_sending_email,
            Searches = document.Searches,
            Scans = document.Scans,
            Consumptions = document.Consumptions
        };
    }

    /// <summary>
    /// Converts a UserDto object to a UserDocument object.
    /// </summary>
    /// <param name="dto">The UserDto object to convert.</param>
    /// <returns>A UserDocument object populated with data from the UserDto object.</returns>
    public static UserDocument FromDto(UserDto dto){
        return new UserDocument{
            _id = dto._id,
            userId = dto.userId,
            email = dto.email,
            first_name = dto.first_name,
            last_name = dto.last_name,
            min_nutri_score = dto.min_nutri_score,
            min_eco_score = dto.min_eco_score,
            country = dto.country,
            cuisine_type = dto.cuisine_type,
            allergens = dto.allergens,
            is_sending_email = dto.is_sending_email,
            Searches = dto.Searches,
            Scans = dto.Scans,
            Consumptions = dto.Consumptions
        };
    }
}