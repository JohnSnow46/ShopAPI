using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Products;

public class CreateProductUseCase(IProductRepository productRepository)
{
    public async Task<Result<ProductDto>> ExecuteAsync(CreateProductDto dto)
    {
        if (dto.Price <= 0)
            return Result<ProductDto>.Failure("Price must be greater than 0.");

        if (dto.StockQuantity < 0)
            return Result<ProductDto>.Failure("Stock quantity cannot be negative.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await productRepository.AddAsync(product);

        return Result<ProductDto>.Success(new ProductDto(
            created.Id, created.Name, created.Description, created.Price,
            created.StockQuantity, created.Category, created.ImageUrl, created.IsActive));
    }
}
