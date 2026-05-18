using ECommerce.Application.Interfaces; // inherit interface
using ECommerce.Application.DTOs.Query;
using ECommerce.Domain.Entities;        // uses the entity product
using ECommerce.Infrastructure.Persistence; // inject the DbContext
using Microsoft.EntityFrameworkCore;    //to deal with DB


namespace ECommerce.Infrastructure.Repositories
{
    //low-level module
    public class ProductRepository : IProductRepository
    {

        private readonly AppDbContext _context;

        //This is classic Constructor Injection, a type of Dependency Injection.
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(QueryParameters queryParameters)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryParameters.Search))
            {
                var search = queryParameters.Search.Trim();
                var searchDate = DateTime.TryParse(search, out var parsed) ? parsed.Date : (DateTime?)null;

                query = searchDate.HasValue
                    ? query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Author.Contains(search) ||
                        p.PublishDate.Date == searchDate.Value)
                    : query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Author.Contains(search));
            }

            if (queryParameters.PublishDate.HasValue)
            {
                var date = queryParameters.PublishDate.Value.Date;
                query = query.Where(p => p.PublishDate.Date == date);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.Author))
            {
                var author = queryParameters.Author.Trim();
                query = query.Where(p => p.Author.Contains(author));
            }

            if (queryParameters.MinPrice.HasValue)
                query = query.Where(p => p.Price >= queryParameters.MinPrice.Value);

            if (queryParameters.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= queryParameters.MaxPrice.Value);

            if (queryParameters.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == queryParameters.CategoryId.Value);

            query = queryParameters.SortBy?.ToLowerInvariant() switch
            {
                "publishdate_asc" => query.OrderBy(p => p.PublishDate),
                "publishdate_desc" => query.OrderByDescending(p => p.PublishDate),
                "author_asc" => query.OrderBy(p => p.Author),
                "author_desc" => query.OrderByDescending(p => p.Author),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "category_asc" => query.OrderBy(p => p.Category != null ? p.Category.Name : string.Empty),
                "category_desc" => query.OrderByDescending(p => p.Category != null ? p.Category.Name : string.Empty),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id) => await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);


        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            // Remove cart items that reference this product (FK is Restrict, so we must delete them first)
            var cartItems = await _context.CartItems.Where(c => c.ProductId == product.Id).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
