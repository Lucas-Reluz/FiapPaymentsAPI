using Microsoft.EntityFrameworkCore;
using PaymentsAPI.Domain.Entities;
using PaymentsAPI.Domain.Interfaces;
using PaymentsAPI.Infrastructure.Data;

namespace PaymentsAPI.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PaymentsDbContext _context;

    public OrderRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;

        return await _context.Orders
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .CountAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id);
    }
}
