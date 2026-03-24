using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopAPI.Domain.Entities;

namespace ShopAPI.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.UserId, c.ProductId })
            .IsUnique();
    }
}
