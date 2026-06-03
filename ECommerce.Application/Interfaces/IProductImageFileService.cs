namespace ECommerce.Application.Interfaces;

public interface IProductImageFileService
{
    void TryDelete(string? imageUrl);
}
