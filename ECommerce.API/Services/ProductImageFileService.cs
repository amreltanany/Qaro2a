using ECommerce.Application.Interfaces;

namespace ECommerce.API.Services;

public class ProductImageFileService : IProductImageFileService
{
    private readonly IWebHostEnvironment _environment;

    public ProductImageFileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void TryDelete(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var normalized = imageUrl.Replace('\\', '/').Trim();
        const string prefix = "/images/products/";
        if (!normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        var fileName = Path.GetFileName(normalized);
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..", StringComparison.Ordinal))
            return;

        var filePath = Path.Combine(_environment.WebRootPath, "images", "products", fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
