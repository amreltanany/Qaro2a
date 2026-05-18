using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IPublishRepository
{
    Task<Publish?> GetByIdAsync(int id);
    Task<IEnumerable<Publish>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Publish>> GetAllAsync();
    Task AddAsync(Publish publish);
}
