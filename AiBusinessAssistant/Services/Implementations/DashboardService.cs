using AiBusinessAssistant.DTOs.Dashboard;
using AiBusinessAssistant.DTOs.Order;
using AiBusinessAssistant.DTOs.Product;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Repositories.Interfaces;
using AiBusinessAssistant.Services.Interfaces;

namespace AiBusinessAssistant.Services.Implementations;

public class DashboardService : IDashboardService
{
    private const int RecentOrderCount = 5;
    private const int LowStockThreshold = 5;
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var totalCustomers = await _dashboardRepository.GetTotalCustomersAsync(cancellationToken);
        var totalOrders = await _dashboardRepository.GetTotalOrdersAsync(cancellationToken);
        // SQLite cannot translate SUM(decimal), so sum the stored decimal values in .NET.
        var orderTotals = await _dashboardRepository.GetOrderTotalsAsync(cancellationToken);
        var pendingOrders = await _dashboardRepository.GetPendingOrdersCountAsync(cancellationToken);
        var recentOrders = await _dashboardRepository.GetRecentOrdersAsync(RecentOrderCount, cancellationToken);
        var lowStockProducts = await _dashboardRepository.GetLowStockProductsAsync(LowStockThreshold, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalCustomers = totalCustomers,
            TotalOrders = totalOrders,
            Revenue = orderTotals.Sum(),
            PendingOrders = pendingOrders,
            RecentOrders = recentOrders.Select(MapOrder).ToList(),
            LowStockProducts = lowStockProducts.Select(MapProduct).ToList()
        };
    }

    private static OrderDto MapOrder(Order order) => new()
    {
        OrderId = order.OrderId,
        OrderNumber = order.OrderNumber,
        CustomerId = order.CustomerId,
        CustomerName = order.Customer?.Name ?? string.Empty,
        OrderDate = order.OrderDate,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        OrderItems = order.OrderItems.Select(item => new OrderItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList()
    };

    private static ProductDto MapProduct(Product product) => new()
    {
        ProductId = product.ProductId,
        Name = product.Name,
        SKU = product.SKU,
        Description = product.Description,
        UnitPrice = product.UnitPrice,
        StockQuantity = product.StockQuantity,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt
    };
}
