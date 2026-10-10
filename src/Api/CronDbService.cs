using System.Net.Http.Headers;
using System.Text.Json;
using Cronos;

namespace EcoGuide.Api
{
    /// <summary>
    /// A background service that periodically deletes unverified users from the Auth0 database based on a cron expression.
    /// </summary>
    /// <param name="client">The HTTP client used for making requests to the Auth0 API.</param>
    /// <param name="logger">The logger used for logging information and errors.</param>
    /// <param name="configuration">The configuration used for retrieving Auth0 settings.</param>
    public class CronDbService(HttpClient client, ILogger<CronDbService> logger, IConfiguration configuration) : BackgroundService
    {
        private readonly HttpClient _client = client;
        private readonly ILogger<CronDbService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly CronExpression _cronExpression = CronExpression.Parse("*/3 * * * *");

        /// <summary>
        /// Executes the background task that periodically deletes unverified users based on a cron expression.
        /// </summary>
        /// <param name="stoppingToken">A token that can be used to signal cancellation of the task.</param>
        /// <returns>A task that represents the background execution operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRun = _cronExpression.GetNextOccurrence(DateTime.UtcNow);
                if (nextRun.HasValue)
                {
                    var delay = nextRun.Value - DateTime.UtcNow;
                    await Task.Delay(delay, stoppingToken);
                    await DeleteUnverifiedUsers();
                }
            }
        }

        /// <summary>
        /// Retrieves an Auth0 management token using client credentials.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the Auth0 management token as a string.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the access token is null or empty, or when there is an error while retrieving the management token.
        /// </exception>
        private async Task<string> GetAuth0ManagementToken()
        {
            var domain = _configuration["Auth0:Domain"];
            var clientId = _configuration["Auth0:ClientId"];
            var clientSecret = _configuration["Auth0:ClientSecret"];
            var audienceRegister = _configuration["Auth0:Audience_register"];

            var url = $"https://{domain}/oauth/token";
            var response = await _client.PostAsync(url, new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("audience", audienceRegister),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "read:users")
            ]));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonDocument>(json);
                var accessToken = data?.RootElement.GetProperty("access_token").GetString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is null or empty.");
                }
                return accessToken;
            }
            else
            {
                throw new Exception("Error while retrieving the management token.");
            }
        }

        /// <summary>
        /// Deletes unverified users from the Auth0 database who signed up more than 10 minutes ago.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error in deserializing the JSON document or if the signup date is null or empty.</exception>
        private async Task DeleteUnverifiedUsers()
        {
            try
            {
                var token = await GetAuth0ManagementToken();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var domain = _configuration["Auth0:Domain"];
                _client.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"https://{domain}/api/v2/users?q=email_verified:false&search_engine=v3";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var document = JsonSerializer.Deserialize<JsonDocument>(json) ?? throw new Exception("Failed to deserialize JSON document.");
                    var users = document.RootElement.EnumerateArray();
                    foreach (var user in users)
                    {
                        if (!user.GetProperty("email_verified").GetBoolean())
                        {
                            if (user.TryGetProperty("user_metadata", out var appMetadata))
                            {
                                if (appMetadata.TryGetProperty("signUpDate", out var signupDateProperty))
                                {
                                    Console.WriteLine(user.GetProperty("email"));
                                    Console.WriteLine(user.GetProperty("user_metadata"));

                                    var signupDateString = signupDateProperty.GetString();
                                    if (string.IsNullOrEmpty(signupDateString))
                                    {
                                        throw new Exception("Signup date is null or empty.");
                                    }
                                    var signupDate = DateTime.Parse(signupDateString);
                                    var oneHourAgo = DateTime.Now.AddMinutes(-10);
                                    Console.WriteLine("oneHourAgo {0}", oneHourAgo);
                                    Console.WriteLine("signupDate {0}", signupDate);
                                    Console.WriteLine(DateTime.Now);
                                    if (signupDate < oneHourAgo)
                                    {
                                        var userId = user.GetProperty("user_id").GetString();
                                        var deleteUrl = $"https://{domain}/api/v2/users/{userId}";
                                        var deleteResponse = await _client.DeleteAsync(deleteUrl);

                                        if (deleteResponse.IsSuccessStatusCode)
                                        {
                                            _logger.LogInformation($"User {userId} deleted");
                                        }
                                        else
                                        {
                                            _logger.LogError($"Deletion error {userId}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError("Error while retrieving users.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }
    }
}