using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.API.Extensions;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.UseCases.Cart;

namespace ShopAPI.API.Controllers;

/// <summary>
/// Manages the authenticated user's shopping cart. All endpoints require a valid JWT token.
/// </summary>
[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(
    GetCartUseCase getCartUseCase,
    AddToCartUseCase addToCartUseCase,
    RemoveFromCartUseCase removeFromCartUseCase,
    ClearCartUseCase clearCartUseCase) : ControllerBase
{
    /// <summary>
    /// Returns the current user's cart with all items and the total price.
    /// </summary>
    /// <returns>200 with the cart contents.</returns>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.GetUserId();
        var result = await getCartUseCase.ExecuteAsync(userId);
        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a product to the cart or increments its quantity if already present.
    /// </summary>
    /// <param name="dto">Product ID and quantity to add.</param>
    /// <returns>200 with the updated cart; 400 if the product does not exist or stock is insufficient.</returns>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var userId = User.GetUserId();
        var result = await addToCartUseCase.ExecuteAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a single product from the cart.
    /// </summary>
    /// <param name="productId">GUID of the product to remove.</param>
    /// <returns>204 on success; 400 if the product is not in the cart.</returns>
    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var userId = User.GetUserId();
        var result = await removeFromCartUseCase.ExecuteAsync(userId, productId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Removes all items from the current user's cart.
    /// </summary>
    /// <returns>204 on success.</returns>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.GetUserId();
        var result = await clearCartUseCase.ExecuteAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
