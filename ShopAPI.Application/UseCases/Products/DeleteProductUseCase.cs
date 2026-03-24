using ShopAPI.Application.Common;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Products;

public class DeleteProductUseCase(IProductRepository productRepository)
{
    public async Task<Result<bool>> ExecuteAsync(Guid id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
            return Result<bool>.Failure("Product not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await productRepository.UpdateAsync(product);

        return Result<bool>.Success(true);
    }
}
