using AiBusinessAssistant.Data;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Repositories.Implementations;

public class DashboardRepository : IDashboardRepository
{
    private readonly BusinessAiDbContext _db;

    public DashboardRepository(BusinessAiDbContext db)
    {
        _db = db;
    }

    public Task<int> GetTotalCustomersAsync(CancellationToken cancellationToken) =>
        _db.Customers.CountAsync(cancellationToken);

    public Task<int> GetTotalOrdersAsync(CancellationToken cancellationToken) =>
        _db.Orders.CountAsync(cancellationToken);

    public Task<List<decimal>> GetOrderTotalsAsync(CancellationToken cancellationToken) =>
        _db.Orders
            .AsNoTracking()
            .Select(order => order.TotalAmount)
            .ToListAsync(cancellationToken);

    public Task<int> GetPendingOrdersCountAsync(CancellationToken cancellationToken) =>
        _db.Orders.CountAsync(order => order.Status == "Pending", cancellationToken);

    public Task<List<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken) =>
        _db.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.OrderItems)
            .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.OrderId)
            .Take(count)
            .ToListAsync(cancellationToken);

    public Task<List<Product>> GetLowStockProductsAsync(int stockThreshold, CancellationToken cancellationToken) =>
        _db.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.StockQuantity <= stockThreshold)
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);
}
