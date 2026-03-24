using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShopAPI.API.Extensions;
using ShopAPI.API.Hubs;
using ShopAPI.Application.UseCases.Orders;
using ShopAPI.Domain.Enums;

namespace ShopAPI.API.Controllers;

public record UpdateOrderStatusRequest(OrderStatus Status);

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(
    CreateOrderUseCase createOrderUseCase,
    GetOrdersUseCase getOrdersUseCase,
    GetOrderByIdUseCase getOrderByIdUseCase,
    UpdateOrderStatusUseCase updateOrderStatusUseCase,
    IHubContext<OrderHub> hubContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var userId = User.GetUserId();
        var result = await createOrderUseCase.ExecuteAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        await hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("OrderCreated", result.Value);
        await hubContext.Clients
            .Group("admins")
            .SendAsync("OrderCreated", result.Value);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.GetUserId();
        var result = await getOrdersUseCase.ExecuteAsync(userId, User.IsAdmin());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var userId = User.GetUserId();
        var result = await getOrderByIdUseCase.ExecuteAsync(id, userId, User.IsAdmin());
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await updateOrderStatusUseCase.ExecuteAsync(id, request.Status);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        await hubContext.Clients
            .Group($"user-{result.Value!.UserId}")
            .SendAsync("OrderStatusUpdated", result.Value);
        await hubContext.Clients
            .Group("admins")
            .SendAsync("OrderStatusUpdated", result.Value);

        return Ok(result.Value);
    }
}
