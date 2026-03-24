using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Products;

public class GetProductByIdUseCase(IProductRepository productRepository)
{
    public async Task<Result<ProductDto>> ExecuteAsync(Guid id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
            return Result<ProductDto>.Failure("Product not found.");

        return Result<ProductDto>.Success(new ProductDto(
            product.Id, product.Name, product.Description, product.Price,
            product.StockQuantity, product.Category, product.ImageUrl, product.IsActive));
    }
}
