using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Enums;

namespace ShopAPI.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<OrderStatus>(s))
            .HasMaxLength(20);

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
