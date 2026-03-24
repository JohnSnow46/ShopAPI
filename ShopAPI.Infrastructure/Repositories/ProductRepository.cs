using Microsoft.EntityFrameworkCore;
using ShopAPI.Domain.Common;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;
using ShopAPI.Infrastructure.Data;

namespace ShopAPI.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id) =>
        await db.Products.FindAsync(id);

    public async Task<PagedResult<Product>> GetAllAsync(
        string? category = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = db.Products.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)));

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Product> AddAsync(Product product)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        db.Products.Update(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is not null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateStockAsync(Guid id, int quantity)
    {
        await db.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, quantity));
    }
}
