using Microsoft.EntityFrameworkCore;
using ShopAPI.Domain.Entities;
using ShopAPI.Domain.Interfaces;
using ShopAPI.Infrastructure.Data;

namespace ShopAPI.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id) =>
        await db.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
