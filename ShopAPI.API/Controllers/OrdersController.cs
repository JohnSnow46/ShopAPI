using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShopAPI.API.Extensions;
using ShopAPI.API.Hubs;
using ShopAPI.Application.UseCases.Orders;
using ShopAPI.Domain.Enums;

namespace ShopAPI.API.Controllers;

/// <summary>
/// Request payload for updating an order's status.
/// </summary>
/// <param name="Status">The new <see cref="OrderStatus"/> value.</param>
public record UpdateOrderStatusRequest(OrderStatus Status);

/// <summary>
/// Manages orders. Regular users see only their own orders; admins see all orders.
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(
    CreateOrderUseCase createOrderUseCase,
    GetOrdersUseCase getOrdersUseCase,
    GetOrderByIdUseCase getOrderByIdUseCase,
    UpdateOrderStatusUseCase updateOrderStatusUseCase,
    IHubContext<OrderHub, IOrderHubClient> hubContext) : ControllerBase
{
    /// <summary>
    /// Creates an order from the current user's cart contents and clears the cart on success.
    /// Sends a <c>NewOrderPlaced</c> SignalR event to admins, and <c>StockLow</c> alerts for
    /// any products that dropped below the low-stock threshold.
    /// </summary>
    /// <returns>201 with the created order; 400 if the cart is empty.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var userId = User.GetUserId();
        var result = await createOrderUseCase.ExecuteAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var order = result.Value!.Order;
        await hubContext.Clients.Group("admins").NewOrderPlaced(order);

        foreach (var lowStock in result.Value.LowStockItems)
            await hubContext.Clients.Group("admins").StockLow(lowStock.ProductId, lowStock.ProductName, lowStock.NewQuantity);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>
    /// Returns a list of orders. Users receive only their own orders; admins receive all orders.
    /// </summary>
    /// <returns>200 with the list of orders.</returns>
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.GetUserId();
        var result = await getOrdersUseCase.ExecuteAsync(userId, User.IsAdmin());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns a single order by its identifier. Users can only access their own orders.
    /// </summary>
    /// <param name="id">Order GUID.</param>
    /// <returns>200 with order details; 404 if not found or the caller is not authorised to view it.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var userId = User.GetUserId();
        var result = await getOrderByIdUseCase.ExecuteAsync(id, userId, User.IsAdmin());
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Updates the status of an order. Requires Admin role.
    /// Sends an <c>OrderStatusChanged</c> SignalR event to the owning user and,
    /// for terminal states (Confirmed, Shipped, Delivered), to all admins.
    /// </summary>
    /// <param name="id">Order GUID.</param>
    /// <param name="request">The new status value.</param>
    /// <returns>200 with the updated order; 400 on invalid status transition.</returns>
    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await updateOrderStatusUseCase.ExecuteAsync(id, request.Status);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var order = result.Value!;
        await hubContext.Clients.Group($"user-{order.UserId}").OrderStatusChanged(order);

        if (request.Status is OrderStatus.Confirmed or OrderStatus.Shipped or OrderStatus.Delivered)
            await hubContext.Clients.Group("admins").OrderStatusChanged(order);

        return Ok(order);
    }
}
