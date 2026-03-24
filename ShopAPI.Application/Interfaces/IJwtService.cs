using ShopAPI.Domain.Entities;

namespace ShopAPI.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
