using PaymentsAPI.Domain.Entities;

namespace PaymentsAPI.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<int> CountByUserIdAsync(Guid userId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> ExistsAsync(Guid id);
}
