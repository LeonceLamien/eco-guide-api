using System.Net.Http.Headers;
using System.Text.Json;
using MongoDB.Driver;
using System.Text;

using EcoGuide.Data.Documents.Users;
using EcoGuide.Data.Exceptions;
using EcoGuide.Data.Constants;
using EcoGuide.Data.Dtos;

namespace EcoGuide.Data.Repositories;

/// <summary>
/// Handle data operations on <see cref="UserDto"/>
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get a User asynchronously by its ID.
    /// </summary>
    /// <param name="id">The ID of the User to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the User document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the User with the specified ID is not found.</exception>
    Task<UserDto> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user asynchronously by its email.
    /// </summary>
    /// <param name="email">The ID of the user to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the user document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the user with the specified ID is not found.</exception>
    Task<bool> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signup a User asynchronously by its ID.
    /// </summary>
    /// <param name="userData">User information.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the User document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the User with the specified ID is not found.</exception>
    Task<UserDto> SignUpAsync(UserDto userData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a User asynchronously by its ID.
    /// </summary>
    /// <param name="userData">User informations.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the User document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the User with the specified ID is not found.</exception>
    Task<UserDto> UpdateAsync(UserDto userData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a User asynchronously by its email.
    /// </summary>
    /// <param name="userId">The data of the User to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the User document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the User with the specified ID is not found.</exception>
    Task<UserDto> UpdateIsSendingEmailAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user asynchronously by its ID.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the user document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the user with the specified ID is not found.</exception>
    Task<bool> DeleteUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user asynchronously by its ID.
    /// </summary>
    /// <param name="emailPayload">The content of the mail.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="UserDto"/> mapped from the user document.</returns>
    /// <exception cref="DocumentNotFoundException">Thrown when the user with the specified ID is not found.</exception>
    Task<bool> SendEmailAsync(Dictionary<string, object> emailPayload, CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public class UserRepository : IUserRepository
{
    private IMongoCollection<UserDocument> Users { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="client">The MongoDB client used to access the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="client"/> is null.</exception>
    public UserRepository(IMongoClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        var database = client.GetDatabase(Databases.EcoGuide.Name);
        Users = database.GetCollection<UserDocument>(Databases.EcoGuide.Collections.Users);
    }

    /// <inheritdoc/>
    public async Task<UserDto> GetAsync(string id, CancellationToken cancellationToken)
    {
        var User = await Users
            .Find(p => p.userId == id)
            .FirstOrDefaultAsync(cancellationToken);
        if (User == null)
        {
            return null;
        }
        return UserDto.FromDocument(User);
    }

    /// <inheritdoc/>
    public async Task<bool> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var User = await Users
            .Find(p => p.email == email)
            .FirstOrDefaultAsync(cancellationToken);
        return User != null;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var deleteResult = await Users
            .DeleteOneAsync(p => p.userId == userId, cancellationToken);
        return deleteResult.DeletedCount > 0;
    }

    /// <inheritdoc/>
    public async Task<UserDto> SignUpAsync(UserDto userData, CancellationToken cancellationToken)
    {
        UserDocument newUser = UserDto.FromDto(userData);
        await Users.InsertOneAsync(newUser);
        if (newUser == null)
        {
            throw new DocumentNotFoundException<UserDocument>(userData.userId);
        }
        return UserDto.FromDocument(newUser);
    }

    /// / <inheritdoc/>
    public async Task<UserDto> UpdateAsync(UserDto userData, CancellationToken cancellationToken)
    {
        UserDocument newUserDocument = UserDto.FromDto(userData);

        // Check if email exists for another user
        var emailExists = await Users.Find(u =>
            u.email == userData.email &&
            u.userId != userData.userId
        ).AnyAsync();

        if (emailExists)
        {
            return null;
        }

        var filter = Builders<UserDocument>.Filter.Eq(doc => doc.userId, userData.userId);

        var update = Builders<UserDocument>.Update
            .Set(doc => doc.first_name, userData.first_name)
            .Set(doc => doc.last_name, userData.last_name)
            .Set(doc => doc.min_nutri_score, userData.min_nutri_score)
            .Set(doc => doc.min_eco_score, userData.min_eco_score)
            .Set(doc => doc.country, userData.country)
            .Set(doc => doc.allergens, userData.allergens)
            .Set(doc => doc.cuisine_type, userData.cuisine_type)
            .Set(doc => doc.is_sending_email, userData.is_sending_email)
            .Set(doc => doc.email, userData.email);

        var result = await Users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            return null;
        }

        return UserDto.FromDocument(newUserDocument);
    }

    /// <inheritdoc/>
    public async Task<bool> SendEmailAsync(Dictionary<string, object> emailPayload, CancellationToken cancellationToken = default)
    {

        string apiKey = Environment.GetEnvironmentVariable("SENDGRIDKEY") ?? throw new InvalidOperationException("Undefined SENDGRIDKEY");
        const string url = "https://api.sendgrid.com/v3/mail/send";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var jsonPayload = JsonSerializer.Serialize(emailPayload);
        var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, requestContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error API: {response.StatusCode}, Details: {errorResponse}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception when sending email: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<UserDto> UpdateIsSendingEmailAsync(string userId, CancellationToken cancellationToken)
    {
        UserDocument newUserDocument = await Users
            .Find(p => p.userId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        var filter = Builders<UserDocument>.Filter.Eq(doc => doc.userId, userId);

        var update = Builders<UserDocument>.Update
            .Set(doc => doc.is_sending_email, !newUserDocument.is_sending_email);

        var result = await Users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new DocumentNotFoundException<UserDocument>(userId);
        }

        return UserDto.FromDocument(newUserDocument);
    }
}