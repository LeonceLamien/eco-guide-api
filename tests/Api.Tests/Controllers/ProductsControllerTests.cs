using EcoGuide.Api.Controllers;
using EcoGuide.Data.Dtos;
using EcoGuide.Data.Dtos.Enums;
using EcoGuide.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EcoGuide.Api.Tests.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetAsync_Should_Call_ProductService_With_Correct_Parameters_And_Return_Product()
    {
        // Arrange
        var productId = "123";
        var cancellationToken = CancellationToken.None;
        var expectedProduct = new ProductDto { Id = productId, Name = "Test Product", Nutriscore = NutriScore.A};

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.GetAsync(productId, cancellationToken))
            .ReturnsAsync(expectedProduct);

        var controller = new ProductsController(productRepositoryMock.Object);

        // Act
        var result = await controller.GetAsync(productId, cancellationToken);

        // Assert

        productRepositoryMock.Verify(x => x.GetAsync(productId, cancellationToken), Times.Once);

        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var product = okResult!.Value;
        product.Should().BeEquivalentTo(expectedProduct);
    }
}