using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShopAPI.Application.DTOs;

namespace ShopAPI.API.Hubs;

public interface IOrderHubClient
{
    Task OrderStatusChanged(OrderDto order);
    Task NewOrderPlaced(OrderDto order);
    Task StockLow(Guid productId, string productName, int quantity);
}

[Authorize]
public class OrderHub : Hub<IOrderHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (Context.User?.IsInRole("Admin") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

        await base.OnConnectedAsync();
    }
}
