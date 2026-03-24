using System.Net;
using System.Net.Http.Json;
using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;

namespace ShopAPI.Tests.Integration;

public class ProductsControllerTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Should_Return200_WithProducts_WhenPublicRequest()
    {
        var response = await Client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task Should_Return401_WhenCreatingProductWithoutAuth()
    {
        var dto = new { Name = "Test", Price = 10.0, StockQuantity = 5, Category = "Test" };

        var response = await Client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return403_WhenCreatingProductAsCustomer()
    {
        var token = await RegisterAndGetTokenAsync(UniqueEmail());
        var client = CreateAuthenticatedClient(token);

        var dto = new { Name = "Test", Price = 10.0, StockQuantity = 5, Category = "Test" };
        var response = await client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return201_WhenCreatingProductAsAdmin()
    {
        var adminToken = await GetAuthTokenAsync("admin@shop.com", "Admin123!");
        var client = CreateAuthenticatedClient(adminToken);

        var dto = new { Name = "New Product", Description = "Desc", Price = 99.99, StockQuantity = 10, Category = "Electronics", ImageUrl = (string?)null };
        var response = await client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal("New Product", product.Name);
    }

    [Fact]
    public async Task Should_Return404_WhenProductNotFound()
    {
        var response = await Client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
