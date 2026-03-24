using ShopAPI.Application.Common;
using ShopAPI.Application.DTOs;
using ShopAPI.Domain.Interfaces;

namespace ShopAPI.Application.UseCases.Products;

public class GetProductsUseCase(IProductRepository productRepository)
{
    public async Task<PagedResult<ProductDto>> ExecuteAsync(
        string? category = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var paged = await productRepository.GetAllAsync(category, searchTerm, pageNumber, pageSize);

        return new PagedResult<ProductDto>
        {
            Items = paged.Items.Select(p => new ProductDto(
                p.Id, p.Name, p.Description, p.Price,
                p.StockQuantity, p.Category, p.ImageUrl, p.IsActive)),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
