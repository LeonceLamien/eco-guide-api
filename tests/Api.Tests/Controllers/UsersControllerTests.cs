using EcoGuide.Api.Controllers;
using EcoGuide.Data.Dtos;
using EcoGuide.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EcoGuide.Api.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new UserController(_userRepositoryMock.Object);

        var userClaims = new List<Claim> { new("user_id", "user_123") };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
    }

    [Fact]
    public async Task GetAsync_Should_Call_UserRepository_With_Correct_Parameters_And_Return_User()
    {
        // Arrange
        var userId = "user_123";
        var expectedUser = new UserDto { userId = userId, first_name = "John", last_name = "Doe", email = "john.doe@example.com" };

        _userRepositoryMock
            .Setup(x => x.GetAsync(userId, CancellationToken.None))
            .ReturnsAsync(expectedUser);

        // Act
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = principal
        };
        var result = await _controller.GetUser();

        // Assert
        _userRepositoryMock.Verify(x => x.GetAsync(userId, CancellationToken.None), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var user = okResult!.Value;
        user.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        var userId = "user_123";
        UserDto expectedUser = null;

        _userRepositoryMock
            .Setup(x => x.GetAsync(userId, CancellationToken.None))
            .ReturnsAsync(expectedUser);

        // Act
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = principal
        };
        var result = await _controller.GetUser();

        // Assert
        _userRepositoryMock.Verify(x => x.GetAsync(userId, CancellationToken.None), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Register_ValidUser_Should_Return_Ok()
    {
        // Arrange
        var userDto = new UserDto { userId = "user123", first_name = "Alice", last_name = "Smith", email = "alice.smith@example.com" };

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        _userRepositoryMock.Verify(x => x.SignUpAsync(userDto, CancellationToken.None), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task Register_NullUser_Should_Return_BadRequest()
    {
        // Arrange
        UserDto userDto = null;

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        _userRepositoryMock.Verify(x => x.SignUpAsync(userDto, CancellationToken.None), Times.Never);
        result.Should().NotBeNull();
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid data.");
    }
}