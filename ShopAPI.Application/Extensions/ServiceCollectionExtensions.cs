using Microsoft.Extensions.DependencyInjection;
using ShopAPI.Application.UseCases.Auth;
using ShopAPI.Application.UseCases.Cart;
using ShopAPI.Application.UseCases.Orders;
using ShopAPI.Application.UseCases.Products;

namespace ShopAPI.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetProductsUseCase>();
        services.AddScoped<GetProductByIdUseCase>();
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<UpdateProductUseCase>();
        services.AddScoped<DeleteProductUseCase>();

        services.AddScoped<GetCartUseCase>();
        services.AddScoped<AddToCartUseCase>();
        services.AddScoped<RemoveFromCartUseCase>();
        services.AddScoped<ClearCartUseCase>();

        services.AddScoped<CreateOrderUseCase>();
        services.AddScoped<GetOrdersUseCase>();
        services.AddScoped<GetOrderByIdUseCase>();
        services.AddScoped<UpdateOrderStatusUseCase>();

        services.AddScoped<RegisterUseCase>();
        services.AddScoped<LoginUseCase>();

        return services;
    }
}
