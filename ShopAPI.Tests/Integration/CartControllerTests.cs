using System.Net;
using System.Net.Http.Json;
using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;

namespace ShopAPI.Tests.Integration;

public class CartControllerTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    private async Task<Guid> GetFirstProductIdAsync()
    {
        var response = await Client.GetAsync("/api/products?pageSize=1");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        return result!.Items.First().Id;
    }

    [Fact]
    public async Task Should_AddItemToCart_AndReturn200()
    {
        var productId = await GetFirstProductIdAsync();
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/cart/items", new { productId, quantity = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CartItemDto>();
        Assert.NotNull(item);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task Should_Return400_WhenAddingInactiveProduct()
    {
        // Create an inactive product as admin, then update it to inactive via UpdateProduct
        var adminToken = await GetAuthTokenAsync("admin@shop.com", "Admin123!");
        var adminClient = CreateAuthenticatedClient(adminToken);

        var createDto = new
        {
            Name = "Inactive Product",
            Description = "Will be deactivated",
            Price = 10.0,
            StockQuantity = 5,
            Category = "Test",
            ImageUrl = (string?)null
        };
        var createResponse = await adminClient.PostAsJsonAsync("/api/products", createDto);
        createResponse.EnsureSuccessStatusCode();
        var product = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Deactivate by updating with IsActive = false
        var updateDto = new
        {
            Name = product!.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            IsActive = false
        };
        var updateResponse = await adminClient.PutAsJsonAsync($"/api/products/{product.Id}", updateDto);
        updateResponse.EnsureSuccessStatusCode();

        // Customer tries to add inactive product to cart
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/cart/items", new { productId = product.Id, quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ClearCart_AfterOrderPlaced()
    {
        var productId = await GetFirstProductIdAsync();
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        // Add item to cart
        var addResponse = await client.PostAsJsonAsync("/api/cart/items", new { productId, quantity = 1 });
        addResponse.EnsureSuccessStatusCode();

        // Place order
        var orderResponse = await client.PostAsync("/api/orders", null);
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        // Cart should be empty now
        var cartResponse = await client.GetAsync("/api/cart");
        Assert.Equal(HttpStatusCode.OK, cartResponse.StatusCode);

        var cart = await cartResponse.Content.ReadFromJsonAsync<IEnumerable<CartItemDto>>();
        Assert.NotNull(cart);
        Assert.Empty(cart);
    }
}
