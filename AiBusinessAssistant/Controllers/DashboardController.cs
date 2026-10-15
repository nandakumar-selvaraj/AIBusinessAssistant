using AiBusinessAssistant.Data;
using AiBusinessAssistant.DTOs.Dashboard;
using AiBusinessAssistant.DTOs.Order;
using AiBusinessAssistant.DTOs.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private const int LowStockThreshold = 5;
        private readonly BusinessAiDbContext _db;

        public DashboardController(BusinessAiDbContext db)
        {
            _db = db;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            var totalCustomers = await _db.Customers.CountAsync(cancellationToken);
            // SQLite cannot translate SUM(decimal), so aggregate the selected decimal values in .NET.
            var orderTotals = await _db.Orders
                .AsNoTracking()
                .Select(order => order.TotalAmount)
                .ToListAsync(cancellationToken);
            var totalOrders = orderTotals.Count;
            var revenue = orderTotals.Sum();
            var pendingOrders = await _db.Orders.CountAsync(
                order => order.Status == "Pending", cancellationToken);

            var recentOrders = await _db.Orders
                .AsNoTracking()
                .OrderByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.OrderId)
                .Take(5)
                .Select(order => new OrderDto
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer.Name,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount
                })
                .ToListAsync(cancellationToken);

            var lowStockProducts = await _db.Products
                .AsNoTracking()
                .Where(product => product.IsActive && product.StockQuantity <= LowStockThreshold)
                .OrderBy(product => product.StockQuantity)
                .ThenBy(product => product.Name)
                .Select(product => new ProductDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    SKU = product.SKU,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Ok(new DashboardSummaryDto
            {
                TotalCustomers = totalCustomers,
                TotalOrders = totalOrders,
                Revenue = revenue,
                PendingOrders = pendingOrders,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts
            });
        }
    }
}
