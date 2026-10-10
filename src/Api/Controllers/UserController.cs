using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using EcoGuide.Data.Repositories;
using EcoGuide.Api.Filters;
using EcoGuide.Data.Dtos;

namespace EcoGuide.Api.Controllers;

/// <summary>
/// Controller for managing user-related operations.
/// </summary>
/// <remarks>
/// This controller provides endpoints for user registration, retrieval, update, email status update, 
/// email retrieval, logout, user deletion, and email sending. It uses the "AllowSpecificOrigin" CORS policy 
/// for all endpoints and requires authorization for access.
/// </remarks>
[ApiController]
[ApiExceptionFilter]
[Route("api/user")]
[Authorize]
public class UserController(IUserRepository userService) : ControllerBase
{
    private IUserRepository UserRepository { get; set; } = userService;

    /// <summary>
    /// Registers a new user with the provided user data.
    /// </summary>
    /// <param name="model">The user data transfer object containing user details.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the registration process.
    /// Returns <see cref="BadRequest"/> if the provided data is invalid.
    /// Returns <see cref="Ok"/> with the user data if the registration is successful.
    [EnableCors("AllowSpecificOrigin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto model)
    {
        if (model == null)
        {
            return BadRequest("Invalid data.");
        }
        await UserRepository.SignUpAsync(model);
        return Ok(model);
    }

    /// <summary>
    /// Retrieves the user information based on the Auth0 access token.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the user information if found, otherwise a NotFound result.
    /// </returns>
    /// <remarks>
    /// This method extracts the user ID from the Auth0 access token and searches for the user in the MongoDB database.
    /// </remarks>
    [EnableCors("AllowSpecificOrigin")]
    [HttpPost("get")]
    public async Task<IActionResult> GetUser()
    {
        // Get user ID from Auth0 access token and search in MongoDB
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID not found.");
        }
        var user = await UserRepository.GetAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    /// <summary>
    /// Updates the user information.
    /// </summary>
    /// <param name="model">The user data transfer object containing updated user information.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    /// <remarks>
    /// This method is an HTTP POST endpoint that updates user information in the repository.
    /// It uses the "AllowSpecificOrigin" CORS policy.
    /// </remarks>
    [EnableCors("AllowSpecificOrigin")]
    [HttpPost("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto model)
    {
        UserDto user = await UserRepository.UpdateAsync(model);
        if (user == null){
            return Unauthorized();
        }
        return Ok(user);
    }

    /// <summary>
    /// Updates the email sending status for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose email sending status is to be updated.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    /// <response code="200">If the email sending status was successfully updated.</response>
    /// <response code="400">If the userId is invalid.</response>
    /// <response code="500">If there was an internal server error.</response>
    [EnableCors("AllowSpecificOrigin")]
    [HttpPut("update/{userId}")]
    public async Task<IActionResult> UpdateIsSendingEmailAPI(string userId)
    {
        return Ok(await UserRepository.UpdateIsSendingEmailAsync(userId));
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <param name="ct">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>An <see cref="IActionResult"/> indicating whether the email is present in the user repository.</returns>
    /// <response code="200">Returns a boolean indicating if the email is present.</response>
    [EnableCors("AllowSpecificOrigin")]
    [HttpGet]
    [Route("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email, CancellationToken ct)
    {
        bool isEmailPresent = await UserRepository.GetUserByEmailAsync(email, ct);
        if (!isEmailPresent){
            return NotFound();
        }
        return Ok(isEmailPresent);
    }

    /// <summary>
    /// Logs out the user by calling the Auth0 logout endpoint.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the request.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the logout operation.
    /// If successful, returns a 200 OK status with a success message.
    /// If unsuccessful, returns the appropriate status code with error details.
    /// If an exception occurs, returns a 500 Internal Server Error with the exception message.
    /// </returns>
    [EnableCors("AllowSpecificOrigin")]
    [HttpGet]
    [Route("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        string domain = "dev-czb66bs1etiil5pq.us.auth0.com"; 
        string logoutUrl = $"https://{domain}/v2/logout";

        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(logoutUrl, ct);
            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Logout successful" });
            }
            else
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new
                {
                    message = "Failed to logout",
                    statusCode = response.StatusCode,
                    error = errorDetails
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during logout", error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be deleted.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    /// <response code="200">Returns true if the user was successfully deleted, otherwise false.</response>
    [EnableCors("AllowSpecificOrigin")]
    [HttpDelete]
    [Route("delete/{userId}")]
    public async Task<IActionResult> DeleteById(string userId, CancellationToken ct)
    {
        bool isDeleted = await UserRepository.DeleteUserByIdAsync(userId, ct);
        if (!isDeleted){
            return NotFound();
        }
        return Ok(isDeleted);
    }

    /// <summary>
    /// Sends an email with the specified payload.
    /// </summary>
    /// <param name="emailPayload">A dictionary containing the email payload data.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the email sending operation.</returns>
    /// <remarks>
    /// This method enables CORS for the specified origin and handles HTTP POST requests to the "send" endpoint.
    /// </remarks>
    [EnableCors("AllowSpecificOrigin")]
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] Dictionary<string, object> emailPayload)
    {
        return Ok(await UserRepository.SendEmailAsync(emailPayload));
    }
}