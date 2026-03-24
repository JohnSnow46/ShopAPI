using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.UseCases.Products;

namespace ShopAPI.API.Controllers;

/// <summary>
/// Manages the product catalog. Read operations are public; write operations require Admin role.
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController(
    GetProductsUseCase getProductsUseCase,
    GetProductByIdUseCase getProductByIdUseCase,
    CreateProductUseCase createProductUseCase,
    UpdateProductUseCase updateProductUseCase,
    DeleteProductUseCase deleteProductUseCase) : ControllerBase
{
    /// <summary>
    /// Returns a paged list of products with optional filtering and search.
    /// </summary>
    /// <param name="category">Filter by category name (case-insensitive, optional).</param>
    /// <param name="search">Full-text search on product name and description (optional).</param>
    /// <param name="page">Page number, 1-based (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10).</param>
    /// <returns>200 with a <c>PagedResult</c> containing the matching products.</returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await getProductsUseCase.ExecuteAsync(category, search, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single product by its identifier.
    /// </summary>
    /// <param name="id">Product GUID.</param>
    /// <returns>200 with product data; 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await getProductByIdUseCase.ExecuteAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new product. Requires Admin role.
    /// </summary>
    /// <param name="dto">Product creation payload (name, description, price, stock, category).</param>
    /// <returns>201 with the created product; 400 on validation error.</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await createProductUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetProduct), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing product. Requires Admin role.
    /// </summary>
    /// <param name="id">Product GUID.</param>
    /// <param name="dto">Updated product data.</param>
    /// <returns>200 with the updated product; 404 if not found.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var result = await updateProductUseCase.ExecuteAsync(id, dto);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a product by its identifier. Requires Admin role.
    /// </summary>
    /// <param name="id">Product GUID.</param>
    /// <returns>204 on success; 404 if not found.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await deleteProductUseCase.ExecuteAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return NoContent();
    }
}
