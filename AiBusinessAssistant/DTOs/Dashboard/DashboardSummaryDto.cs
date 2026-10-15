using AiBusinessAssistant.DTOs.Order;
using AiBusinessAssistant.DTOs.Product;

namespace AiBusinessAssistant.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public decimal Revenue { get; set; }
    public int PendingOrders { get; set; }
    public List<OrderDto> RecentOrders { get; set; } = new();
    public List<ProductDto> LowStockProducts { get; set; } = new();
}
