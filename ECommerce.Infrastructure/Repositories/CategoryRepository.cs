using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    // List only: categories without their products (a catalogue of thousands of products must not be loaded here).
    public async Task<IEnumerable<Category>> GetAllAsync() => await _context.Categories.AsNoTracking().ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) => await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
