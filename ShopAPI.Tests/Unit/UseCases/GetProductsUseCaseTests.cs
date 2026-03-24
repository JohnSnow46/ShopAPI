using Moq;
using ShopAPI.Application.UseCases.Products;
using ShopAPI.Domain.Common;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Tests.Unit.UseCases;

public class GetProductsUseCaseTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly GetProductsUseCase _sut;

    public GetProductsUseCaseTests()
    {
        _sut = new GetProductsUseCase(_productRepo.Object);
    }

    [Fact]
    public async Task Should_ReturnPagedResult_WithCorrectTotalCount()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "A", Price = 10m, StockQuantity = 5, Category = "Cat", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "B", Price = 20m, StockQuantity = 3, Category = "Cat", IsActive = true },
        };

        _productRepo.Setup(r => r.GetAllAsync(null, null, 1, 20))
            .ReturnsAsync(new PagedResult<Product> { Items = products, TotalCount = 2, PageNumber = 1, PageSize = 20 });

        var result = await _sut.ExecuteAsync();

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact]
    public async Task Should_FilterByCategory_WhenCategoryProvided()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Laptop", Price = 999m, StockQuantity = 2, Category = "Electronics", IsActive = true },
        };

        _productRepo.Setup(r => r.GetAllAsync("Electronics", null, 1, 20))
            .ReturnsAsync(new PagedResult<Product> { Items = products, TotalCount = 1, PageNumber = 1, PageSize = 20 });

        var result = await _sut.ExecuteAsync(category: "Electronics");

        _productRepo.Verify(r => r.GetAllAsync("Electronics", null, 1, 20), Times.Once);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Laptop", result.Items.First().Name);
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenNoProductsMatch()
    {
        _productRepo.Setup(r => r.GetAllAsync("Nonexistent", null, 1, 20))
            .ReturnsAsync(new PagedResult<Product> { Items = [], TotalCount = 0, PageNumber = 1, PageSize = 20 });

        var result = await _sut.ExecuteAsync(category: "Nonexistent");

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}
