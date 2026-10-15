using AiBusinessAssistant.Controllers;
using AiBusinessAssistant.Data;
using AiBusinessAssistant.DTOs.Dashboard;
using AiBusinessAssistant.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AiBusinessAssistant.Tests;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetSummary_ReturnsZerosAndEmptyCollections_WhenDatabaseIsEmpty()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<BusinessAiDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new BusinessAiDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var result = await new DashboardController(db).GetSummary(CancellationToken.None);
        var summary = Assert.IsType<DashboardSummaryDto>(Assert.IsType<OkObjectResult>(result.Result).Value);

        Assert.Equal(0, summary.TotalCustomers);
        Assert.Equal(0, summary.TotalOrders);
        Assert.Equal(0m, summary.Revenue);
        Assert.Equal(0, summary.PendingOrders);
        Assert.Empty(summary.RecentOrders);
        Assert.Empty(summary.LowStockProducts);
    }

    [Fact]
    public async Task GetSummary_CalculatesMetricsAndReturnsRecentOrdersAndLowStockProducts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<BusinessAiDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new BusinessAiDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var customer = new Customer { Name = "Contoso" };
        db.Customers.AddRange(customer, new Customer { Name = "Fabrikam" });
        db.Products.AddRange(
            new Product { Name = "Low stock", SKU = "LOW-1", UnitPrice = 10m, StockQuantity = 5, IsActive = true },
            new Product { Name = "Inactive stock", SKU = "LOW-2", UnitPrice = 10m, StockQuantity = 0, IsActive = false },
            new Product { Name = "Available stock", SKU = "OK-1", UnitPrice = 10m, StockQuantity = 6, IsActive = true });
        db.Orders.AddRange(
            new Order
            {
                OrderNumber = "ORD-001", Customer = customer, OrderDate = new DateTime(2026, 10, 13),
                Status = "Pending", TotalAmount = 35m
            },
            new Order
            {
                OrderNumber = "ORD-002", Customer = customer, OrderDate = new DateTime(2026, 10, 14),
                Status = "Completed", TotalAmount = 20m
            });
        await db.SaveChangesAsync();

        var result = await new DashboardController(db).GetSummary(CancellationToken.None);
        var summary = Assert.IsType<DashboardSummaryDto>(Assert.IsType<OkObjectResult>(result.Result).Value);

        Assert.Equal(2, summary.TotalCustomers);
        Assert.Equal(2, summary.TotalOrders);
        Assert.Equal(55m, summary.Revenue);
        Assert.Equal(1, summary.PendingOrders);
        Assert.Equal(new[] { "ORD-002", "ORD-001" }, summary.RecentOrders.Select(order => order.OrderNumber));
        var lowStock = Assert.Single(summary.LowStockProducts);
        Assert.Equal("LOW-1", lowStock.SKU);
        Assert.Equal(5, lowStock.StockQuantity);
    }
}
