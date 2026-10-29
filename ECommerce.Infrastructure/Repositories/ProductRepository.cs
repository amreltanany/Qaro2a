using ECommerce.Application.Interfaces; // inherit interface
using ECommerce.Application.DTOs.Product;
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

        // Paged, filtered and sorted list, projected straight to the response DTO: only the columns
        // the pages need are read, and no entities are materialised or tracked.
        public async Task<IReadOnlyList<ProductResponseDto>> GetListAsync(QueryParameters queryParameters)
        {
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryParameters.Search))
            {
                // Arabic-friendly search: hamza / ta marbuta / diacritics are ignored, every word must match the
                // book name, author, category or ISBN, and a 4-digit number matches the publish year (2026, 1950...).
                // The catalogue is small, so the match runs in memory on a thin projection and the page query filters by id.
                var words = Normalize(queryParameters.Search).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var candidates = await _context.Products.AsNoTracking()
                    .Select(p => new { p.Id, p.Name, p.Author, p.Isbn, p.PublishDate.Year, Category = p.Category != null ? p.Category.Name : string.Empty })
                    .ToListAsync();
                var ids = candidates.Where(c =>
                {
                    var hay = Normalize(c.Name + ' ' + c.Author + ' ' + c.Category + ' ' + c.Isbn);
                    return words.All(w => (w.Length == 4 && w.All(char.IsDigit) && int.Parse(w) == c.Year) || hay.Contains(w));
                }).Select(c => c.Id).ToList();
                query = query.Where(p => ids.Contains(p.Id));
            }

            if (queryParameters.PublishDate.HasValue)
            {
                var date = queryParameters.PublishDate.Value.Date;
                query = query.Where(p => p.PublishDate >= date && p.PublishDate < date.AddDays(1));
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

            if (queryParameters.TopRatedOnly)
                query = query.Where(p => p.TopRated);

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
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    PublishDate = p.PublishDate,
                    Author = p.Author,
                    TopRated = p.TopRated,
                    Isbn = p.Isbn,
                    Pages = p.Pages,
                    Size = p.Size
                })
                .ToListAsync();
        }

        // lower-case, Arabic-Indic digits to 0-9, no diacritics / tatweel, and one spelling for look-alike letters
        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var ch in s.ToLowerInvariant())
            {
                if ((ch >= 'ً' && ch <= 'ٟ') || ch == 'ٰ' || ch == 'ـ') continue;
                sb.Append(ch switch
                {
                    'أ' or 'إ' or 'آ' or 'ٱ' => 'ا',
                    'ى' => 'ي', 'ئ' => 'ي', 'ؤ' => 'و', 'ة' => 'ه', 'ڤ' => 'ف', 'چ' => 'ج', 'گ' => 'ك', 'پ' => 'ب',
                    >= '٠' and <= '٩' => (char)('0' + (ch - '٠')),
                    >= '۰' and <= '۹' => (char)('0' + (ch - '۰')),
                    _ => char.IsLetterOrDigit(ch) ? ch : ' '
                });
            }
            return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
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
            // Remove dependent rows first because both FK constraints are Restrict.
            var cartItems = await _context.CartItems.Where(c => c.ProductId == product.Id).ToListAsync();
            var wishlistItems = await _context.WishlistItems.Where(w => w.ProductId == product.Id).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);
            _context.WishlistItems.RemoveRange(wishlistItems);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
