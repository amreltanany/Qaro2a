using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IContactRepository
{
    Task<IReadOnlyList<Contact>> GetAllOrderedByNewestAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
}
