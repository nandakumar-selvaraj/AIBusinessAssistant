using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Data;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Repositories.Implementations;
using AiBusinessAssistant.Services.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AiBusinessAssistant.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CompleteAsync_CompletesPendingOrder_AndIsIdempotent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<BusinessAiDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new BusinessAiDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var customer = new Customer { Name = "Contoso" };
        var order = new Order { OrderNumber = "ORD-100", Customer = customer, Status = "Pending" };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var service = new OrderService(
            new OrderRepository(db),
            new CustomerRepository(db),
            new ProductRepository(db));

        var completed = await service.CompleteAsync(order.OrderId);
        var completedAgain = await service.CompleteAsync(order.OrderId);

        Assert.Equal("Completed", completed.Status);
        Assert.Equal("Completed", completedAgain.Status);
        Assert.Equal("Completed", await db.Orders.Where(x => x.OrderId == order.OrderId).Select(x => x.Status).SingleAsync());
    }

    [Fact]
    public async Task CompleteAsync_RejectsCancelledAndMissingOrders()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<BusinessAiDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new BusinessAiDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var cancelledOrder = new Order
        {
            OrderNumber = "ORD-101",
            Customer = new Customer { Name = "Fabrikam" },
            Status = "Cancelled"
        };
        db.Orders.Add(cancelledOrder);
        await db.SaveChangesAsync();
        var service = new OrderService(
            new OrderRepository(db),
            new CustomerRepository(db),
            new ProductRepository(db));

        await Assert.ThrowsAsync<ApiConflictException>(() => service.CompleteAsync(cancelledOrder.OrderId));
        await Assert.ThrowsAsync<ApiNotFoundException>(() => service.CompleteAsync(999));
    }
}
