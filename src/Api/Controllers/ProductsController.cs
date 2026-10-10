using EcoGuide.Api.Filters;
using EcoGuide.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EcoGuide.Api.Controllers;

[ApiController]
[ApiExceptionFilter]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private IProductRepository ProductRepository { get; }

    public ProductsController(IProductRepository productService)
    {
        ProductRepository = productService;
    }

    [HttpGet]
    [Route("{productId}")]
    public async Task<IActionResult> GetAsync(string productId, CancellationToken ct)
    {
        var product = await ProductRepository.GetAsync(productId, ct);

        return Ok(product);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var products = await ProductRepository.ToListAsync(ct);

        return Ok(products);
    }
}