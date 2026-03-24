using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;

namespace ShopAPI.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Products.Any() || db.Users.Any())
            return;

        var now = DateTime.UtcNow;

        db.Products.AddRange(
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop ProBook 15",
                Description = "Wydajny laptop biznesowy z procesorem Intel i7",
                Price = 3499.99m,
                StockQuantity = 10,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Koszulka bawełniana",
                Description = "Wygodna koszulka 100% bawełna, dostępna w wielu kolorach",
                Price = 49.99m,
                StockQuantity = 100,
                Category = "Clothing",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Kawa Arabica 500g",
                Description = "Ziarnista kawa arabica z Etiopii, palona na średnio",
                Price = 39.99m,
                StockQuantity = 50,
                Category = "Food",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@shop.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            CreatedAt = now
        });

        await db.SaveChangesAsync();
    }
}
