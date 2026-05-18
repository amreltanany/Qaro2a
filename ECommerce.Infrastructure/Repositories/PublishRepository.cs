using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class PublishRepository : IPublishRepository
{
    private readonly AppDbContext _context;

    public PublishRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Publish?> GetByIdAsync(int id) =>
        await _context.Publishes
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Publish>> GetByUserIdAsync(string userId) =>
        await _context.Publishes
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Publish>> GetAllAsync() =>
        await _context.Publishes
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Publish publish)
    {
        await _context.Publishes.AddAsync(publish);
        await _context.SaveChangesAsync();
    }
}
