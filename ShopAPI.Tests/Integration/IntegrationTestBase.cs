using System.Net.Http.Headers;
using System.Net.Http.Json;
using ShopAPI.Application.DTOs;

namespace ShopAPI.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> GetAuthTokenAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result!.Token;
    }

    protected HttpClient CreateAuthenticatedClient(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<string> RegisterAndGetTokenAsync(string email, string password = "Test123!")
    {
        await Client.PostAsJsonAsync("/api/auth/register", new { email, password });
        return await GetAuthTokenAsync(email, password);
    }

    protected static string UniqueEmail() => $"user-{Guid.NewGuid():N}@test.com";
}
