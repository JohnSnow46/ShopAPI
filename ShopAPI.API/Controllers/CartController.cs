using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.API.Extensions;
using ShopAPI.Application.DTOs;
using ShopAPI.Application.UseCases.Cart;

namespace ShopAPI.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(
    GetCartUseCase getCartUseCase,
    AddToCartUseCase addToCartUseCase,
    RemoveFromCartUseCase removeFromCartUseCase,
    ClearCartUseCase clearCartUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.GetUserId();
        var result = await getCartUseCase.ExecuteAsync(userId);
        return Ok(result.Value);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var userId = User.GetUserId();
        var result = await addToCartUseCase.ExecuteAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var userId = User.GetUserId();
        var result = await removeFromCartUseCase.ExecuteAsync(userId, productId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

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
