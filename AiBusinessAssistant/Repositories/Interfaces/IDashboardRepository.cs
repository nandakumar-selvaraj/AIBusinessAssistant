using AiBusinessAssistant.Entities;

namespace AiBusinessAssistant.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<int> GetTotalCustomersAsync(CancellationToken cancellationToken);
    Task<int> GetTotalOrdersAsync(CancellationToken cancellationToken);
    Task<List<decimal>> GetOrderTotalsAsync(CancellationToken cancellationToken);
    Task<int> GetPendingOrdersCountAsync(CancellationToken cancellationToken);
    Task<List<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken);
    Task<List<Product>> GetLowStockProductsAsync(int stockThreshold, CancellationToken cancellationToken);
}
