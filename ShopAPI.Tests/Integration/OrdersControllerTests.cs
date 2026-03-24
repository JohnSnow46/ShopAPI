using System.Net;
using System.Net.Http.Json;
using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;

namespace ShopAPI.Tests.Integration;

public class OrdersControllerTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    private async Task<Guid> GetFirstProductIdAsync()
    {
        var response = await Client.GetAsync("/api/products?pageSize=1");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        return result!.Items.First().Id;
    }

    private async Task AddToCartAsync(HttpClient client, Guid productId, int quantity = 1)
    {
        var response = await client.PostAsJsonAsync("/api/cart/items", new { productId, quantity });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Should_Return201_WhenPlacingOrderWithValidCart()
    {
        var productId = await GetFirstProductIdAsync();
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        await AddToCartAsync(client, productId);

        var response = await client.PostAsync("/api/orders", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.NotEmpty(order.Items);
    }

    [Fact]
    public async Task Should_Return400_WhenCartIsEmpty()
    {
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsync("/api/orders", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return400_WhenProductOutOfStock()
    {
        // Create a product with 0 stock as admin
        var adminToken = await GetAuthTokenAsync("admin@shop.com", "Admin123!");
        var adminClient = CreateAuthenticatedClient(adminToken);

        var createDto = new { Name = "Out of Stock Item", Price = 10.0, StockQuantity = 0, Category = "Test" };
        var createResponse = await adminClient.PostAsJsonAsync("/api/products", createDto);
        createResponse.EnsureSuccessStatusCode();
        var product = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Register customer, add 0-stock product to cart, try to order
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        await client.PostAsJsonAsync("/api/cart/items", new { productId = product!.Id, quantity = 1 });

        var response = await client.PostAsync("/api/orders", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return200_WithOnlyOwnOrders_WhenCustomer()
    {
        // Create a product with guaranteed stock to avoid interference from other tests
        var adminToken = await GetAuthTokenAsync("admin@shop.com", "Admin123!");
        var adminClient = CreateAuthenticatedClient(adminToken);
        var productResponse = await adminClient.PostAsJsonAsync("/api/products",
            new { Name = "Isolation Product", Price = 5.0, StockQuantity = 100, Category = "Test" });
        productResponse.EnsureSuccessStatusCode();
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();
        var productId = product!.Id;

        // Two customers each place an order
        var token1 = await RegisterAndGetTokenAsync(UniqueEmail());
        var client1 = CreateAuthenticatedClient(token1);
        await AddToCartAsync(client1, productId);
        var place1 = await client1.PostAsync("/api/orders", null);
        Assert.Equal(HttpStatusCode.Created, place1.StatusCode);

        var token2 = await RegisterAndGetTokenAsync(UniqueEmail());
        var client2 = CreateAuthenticatedClient(token2);
        await AddToCartAsync(client2, productId);
        await client2.PostAsync("/api/orders", null);

        // Customer1 should only see their own orders
        var response = await client1.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>();
        Assert.NotNull(orders);
        var orderList = orders.ToList();
        Assert.True(orderList.Count >= 1);

        // Decode the user ID from the token to verify
        var tokenParts = token1.Split('.');
        var payloadJson = System.Text.Encoding.UTF8.GetString(
            Convert.FromBase64String(tokenParts[1].PadRight((tokenParts[1].Length + 3) / 4 * 4, '=')));
        var payload = System.Text.Json.JsonDocument.Parse(payloadJson);
        var userId = Guid.Parse(payload.RootElement.GetProperty("sub").GetString()!);

        Assert.All(orderList, o => Assert.Equal(userId, o.UserId));
    }

    [Fact]
    public async Task Should_Return200_WithAllOrders_WhenAdmin()
    {
        var adminToken = await GetAuthTokenAsync("admin@shop.com", "Admin123!");
        var adminClient = CreateAuthenticatedClient(adminToken);

        var response = await adminClient.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>();
        Assert.NotNull(orders);
    }
}
