using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Products;

public class UpdateProductUseCase(IProductRepository productRepository)
{
    public async Task<Result<ProductDto>> ExecuteAsync(Guid id, UpdateProductDto dto)
    {
        if (dto.Price <= 0)
            return Result<ProductDto>.Failure("Price must be greater than 0.");

        if (dto.StockQuantity < 0)
            return Result<ProductDto>.Failure("Stock quantity cannot be negative.");

        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
            return Result<ProductDto>.Failure("Product not found.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.Category = dto.Category;
        product.ImageUrl = dto.ImageUrl;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        var updated = await productRepository.UpdateAsync(product);

        return Result<ProductDto>.Success(new ProductDto(
            updated.Id, updated.Name, updated.Description, updated.Price,
            updated.StockQuantity, updated.Category, updated.ImageUrl, updated.IsActive));
    }
}
