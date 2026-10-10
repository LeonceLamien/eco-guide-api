using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Security.Claims;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net;

using MongoDB.Driver;
using MongoDB.Bson;
using dotenv.net;
using RestSharp;

using EcoGuide.Data.Documents.Users;
using EcoGuide.Data.Dtos.Enums;
using EcoGuide.Data.Constants;
using EcoGuide.Data.Dtos;

namespace EcoGuide.Api.Integration.Tests;

/// <summary>
/// A custom authentication handler that bypasses the actual authentication process
/// and provides a predefined set of claims for testing purposes.
/// </summary>
/// <param name="options">The monitor for the authentication scheme options.</param>
/// <param name="logger">The factory used to create logger instances.</param>
/// <param name="encoder">The URL encoder.</param>
/// <param name="timeProvider">The time provider.</param>
/// <returns>A task that represents the result of the authentication process.</returns>
public class NoAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TimeProvider timeProvider) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim("user_id", "user123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));

    }
}

/// <summary>
/// User-related integration tests.
/// </summary>
public class UsersTests : IClassFixture<MongoDbFixture>, IDisposable
{
    // Auth0 API Credentials
    private readonly string domain;
    private readonly string clientId;
    private readonly string secret;
    private readonly string audienceLogin;
    private readonly string audienceRegister;
    private readonly string apiIdentifier;
    private readonly string url;
    
    // Database
    private readonly MongoDbFixture _fixture;
    private readonly HttpClient _client;
    private readonly HttpClient _jwtAuthClient;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<UserDocument> _usersCollection;
    private IServiceCollection servicesManager;

    // Rest HTTP Request
    private async Task<Dictionary<string, dynamic>> SendRestRequest(Method method, string url, Dictionary<string, string> headers, Dictionary<string, dynamic> body)
    {
        var client = new RestClient(url);
        var request = new RestRequest
        {
            Method = method
        };

        // Serialize body dictionary to JSON string
        string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);

        // Add the body content to the request (set to JSON format)
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

        // Add headers to the request
        foreach (var header in headers)
        {
            // Add headers to RestSharp request
            request.AddHeader(header.Key, header.Value);
        }

        // Send the request and return the response
        var response = await client.ExecuteAsync(request);
        var responseDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response.Content);

        return responseDict;
    }

    // Generates Auth0 user, gets his access token and user ID than deletes the user from auth0 database
    private async Task<Dictionary<string, dynamic>> GenerateAuthUserCred(){
        // Access token for management API
        var AccessBody = new Dictionary<string, dynamic>
        {
            { "client_id", clientId },
            { "client_secret", secret },
            { "audience", audienceRegister },
            { "grant_type", "client_credentials" }
        };

        Dictionary<string, dynamic> responseAuth0Access = await SendRestRequest(
            Method.Post, 
            url, 
            new Dictionary<string, string> { { "Content-Type", "application/json" } }, 
            AccessBody
        );

        // Register a new user
        var registerBody = new Dictionary<string, dynamic>
        {
            { "username", "JohnDoe" },
            { "email", "john.doe@example.com" },
            { "password", "A1b1g1d1@ss@" },
            { "connection", "Username-Password-Authentication" },
            { "email_verified", true },
            { "verify_email", true }
        };

        Dictionary<string, dynamic> responseAuth0Register = await SendRestRequest(
            Method.Post, 
            apiIdentifier, 
            new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {responseAuth0Access["access_token"]}" }
            }, 
            registerBody
        );

        // Login with the new user
        var LoginBody = new Dictionary<string, dynamic>
        {
            { "username", "JohnDoe" },
            { "grant_type", "password" },
            { "password", "A1b1g1d1@ss@" },
            { "scope", "openid profile email" },
            { "client_id", clientId },
            { "client_secret", secret },
            { "audience", audienceLogin }
        };

        Dictionary<string, dynamic> responseAuth0Login = await SendRestRequest(
            Method.Post, 
            url, 
            new Dictionary<string, string> { { "Content-Type", "application/json" } }, 
            LoginBody
        );

        // Obtain management API token
        var ManBody = new Dictionary<string, dynamic>
        {
            { "client_id", clientId },
            { "client_secret", secret },
            { "audience", $"https://{domain}/api/v2/" },
            { "grant_type", "client_credentials" }
        };

        Dictionary<string, dynamic> responseManagementToken = await SendRestRequest(
            Method.Post, 
            $"https://{domain}/oauth/token", 
            new Dictionary<string, string> { { "Content-Type", "application/json" } }, 
            ManBody
        );

        // Delete the user using management API
        await SendRestRequest(
            Method.Delete, 
            $"https://{domain}/api/v2/users/{responseAuth0Register["user_id"]}", 
            new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {responseManagementToken["access_token"]}" },
                { "Content-Type", "application/json" }
            }, 
            []
        );

        return new Dictionary<string, dynamic> {
            {"user_id", responseAuth0Register["user_id"]},
            {"access_token", responseAuth0Login["access_token"]},
        };
    }

    public UsersTests(MongoDbFixture fixture)
    {
        var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envFilePath))
        {
            DotEnv.Load();
            Console.WriteLine(".env file found.");
        }
        else
        {
            Console.WriteLine("No .env file found.");
        }
        domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
        clientId = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID");
        secret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET");
        audienceLogin = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE_LOGIN");
        audienceRegister = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE_REGISTER");
        apiIdentifier = Environment.GetEnvironmentVariable("AUTH0_API_IDENTIFIER");
        
        // Make requests without authentification by _client
        url = $"https://{domain}/oauth/token";
        _fixture = fixture;
        var noAuthFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    servicesManager = services;
                    services.RemoveAll<IMongoClient>();
                    services.AddScoped<IMongoClient>(
                        (_) => _fixture.Client);
                    
                    // Add a No Auth scheme for bypassing real authentication
                    services.AddAuthentication("NoAuth")
                        .AddScheme<AuthenticationSchemeOptions, NoAuthHandler>("NoAuth", options => { });
                });
            });
        _client = noAuthFactory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("NoAuth");

        // Make requests with authentification by _jwtAuthClient
        var jwtAuthFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    servicesManager = services;
                    services.RemoveAll<IMongoClient>();
                    services.AddScoped<IMongoClient>(_ => _fixture.Client);
                });
            });
        _jwtAuthClient = jwtAuthFactory.CreateClient();

        _database = _fixture.Client.GetDatabase(Databases.EcoGuide.Name);
        _usersCollection = _database.GetCollection<UserDocument>(Databases.EcoGuide.Collections.Users);
    }

    [Fact]
    public async Task GetAsync_UserExists_ShouldReturnUser()
    {
        // Arrange
        Dictionary<string, dynamic> TempUser = await GenerateAuthUserCred();

        // Add the user to MongoDB
        var id = ObjectId.GenerateNewId().ToString();

        await _usersCollection.InsertOneAsync(new UserDocument
        {
            _id = id,
            userId = TempUser["user_id"],
            first_name = "John",
            last_name = "Doe",
            email = "john.doe@example.com"
        });

        _jwtAuthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", 
            TempUser["access_token"]
        );

        var res = await _jwtAuthClient.PostAsync("api/user/get", new StringContent(""));
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadAsStringAsync();

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        var user = JsonSerializer.Deserialize<UserDto>(content, jsonSerializerOptions);

        Console.WriteLine(content);

        // Assert
        var expectedUser = new UserDto
        {
            _id = id,
            userId = TempUser["user_id"],
            first_name = "John",
            last_name = "Doe",
            email = "john.doe@example.com"
        };

        user.Should().NotBeNull();
        user.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetAsync_UserDoesNotExists_ShouldReturn404()
    {
        // Act
        // Does not creates user in Mongo database
        Dictionary<string, dynamic> TempUser = await GenerateAuthUserCred();
        _jwtAuthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", 
            TempUser["access_token"]
        );

        var res = await _jwtAuthClient.PostAsync("api/user/get", new StringContent(""));

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_ValidUser_ShouldReturnOk()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/user/register", userDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Alice");
        content.Should().Contain("Smith");
        content.Should().Contain("Belgium");
        content.Should().Contain("Omnivore");
        content.Should().Contain("alice.smith@example.com");
        content.Should().Contain("gluten");
    }

    [Fact]
    public async Task Register_InvalidUser_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new UserDto
        {
            // Missing required fields (CuisineType)
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            allergens = ["gluten"],
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/user/register", userDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAsync_UserAfterRegistration_ShouldReturnUser()
    {
        // Arrange
        Dictionary<string, dynamic> TempUser = await GenerateAuthUserCred();

        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = TempUser["user_id"],
            first_name = "Bob",
            last_name = "Brown",
            email = "bob.brown@example.com",
            min_nutri_score = NutriScore.C,
            min_eco_score = EcoScore.E,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = [],
        };
 
        // Register the user
        var registerResponse = await _client.PostAsJsonAsync("api/user/register", userDto);
        registerResponse.EnsureSuccessStatusCode(); // Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK); // Assert

        // Act
        _jwtAuthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", 
            TempUser["access_token"]
        );

        var res = await _jwtAuthClient.PostAsync("api/user/get", new StringContent(""));
        res.EnsureSuccessStatusCode();
        var content = await res.Content.ReadAsStringAsync();
        content.Should().Contain("Bob");
        content.Should().Contain("Brown");
        content.Should().Contain("Belgium");
        content.Should().Contain("Omnivore");
        content.Should().Contain("bob.brown@example.com");
    }

    [Fact]
    public async Task GetAsync_UnauthorizedAccess_ShouldReturn401()
    {
        var res = await _jwtAuthClient.PostAsync("api/user/get", new StringContent(""));
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAsync_UserAfterUpdate_ShouldReturnChangeUser()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        var userDtoUpdate = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice4545", // Update first name
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/user/register", userDto);
        var updateResp = await _client.PostAsJsonAsync("api/user/update", userDtoUpdate);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await updateResp.Content.ReadAsStringAsync();
        content.Should().Contain("Alice4545");
        content.Should().Contain("Smith");
        content.Should().Contain("Belgium");
        content.Should().Contain("Omnivore");
        content.Should().Contain("alice.smith@example.com");
        content.Should().Contain("gluten");
    }

    [Fact]
    public async Task UpdateAsync_UserAfterExistentEmailUpdate_ShouldReturnUnauthorized()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        var userDtoExistent = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user987",
            first_name = "Alice",
            last_name = "Smith",
            email = "adam.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        var userDtoUpdate = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice4545", // Update first name
            last_name = "Smith",
            email = "adam.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("api/user/register", userDto);
        var response2 = await _client.PostAsJsonAsync("api/user/register", userDtoExistent);
        var updateResp = await _client.PostAsJsonAsync("api/user/update", userDtoUpdate);
        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAsync_UserNotExists_ShouldReturnUnauthorized()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        var userDtoUpdate = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice4545", // Update first name
            last_name = "Smith",
            email = "adam.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var updateResp = await _client.PostAsJsonAsync("api/user/update", userDtoUpdate);
        // Assert
        updateResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAsync_UserDelete_ShouldReturnOk()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var registerResponse = await _client.PostAsJsonAsync("api/user/register", userDto);
        var deleteResponse = await _client.DeleteAsync("api/user/delete/user789");
        var GetEmailResponse = await _client.GetAsync("api/user/email/alice.smith@example.com");
        // Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        GetEmailResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_UserDelete_ShouldReturnNotFound()
    {
        // Act
        var deleteResponse = await _client.DeleteAsync("api/user/delete/user789");
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LogoutAsync_UserLogout_ShouldReturnOk()
    {
        // Arrange
        Dictionary<string, dynamic> TempUser = await GenerateAuthUserCred();

        _jwtAuthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", 
            TempUser["access_token"]
        );
        // Act
        var logoutResp = await _jwtAuthClient.GetAsync("api/user/logout");
        // Assert
        logoutResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByEmail_UserGet_ShouldReturnOk()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var registerResponse = await _client.PostAsJsonAsync("api/user/register", userDto);
        var GetEmailResponse = await _client.GetAsync("api/user/email/alice.smith@example.com");
        // Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        GetEmailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByEmail_UserGet_ShouldReturnNotFound()
    {
        // Arrange
        var userDto = new UserDto
        {
            _id = ObjectId.GenerateNewId().ToString(),
            userId = "user789",
            first_name = "Alice",
            last_name = "Smith",
            email = "alice.smith@example.com",
            min_nutri_score = NutriScore.A,
            min_eco_score = EcoScore.B,
            country = "Belgium",
            cuisine_type = "Omnivore",
            allergens = ["gluten"],
        };

        // Act
        var registerResponse = await _client.PostAsJsonAsync("api/user/register", userDto);
        var GetEmailResponse = await _client.GetAsync("api/user/email/alice.smith2545@example.com");
        // Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        GetEmailResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    public void Dispose()
    {
        var database = _fixture.Client.GetDatabase(Databases.EcoGuide.Name);
        var usersCollection = database.GetCollection<UserDocument>(Databases.EcoGuide.Collections.Users);
        usersCollection.DeleteManyAsync(_ => true).Wait();
    }
}
