using System.ComponentModel;
using AiBusinessAssistant.Services.Interfaces;
using Microsoft.SemanticKernel;

namespace AiBusinessAssistant.Features.AI.Plugins;

public sealed class OrderPlugin(IOrderService orders)
{
    private const int MaximumResults = 20;

    [KernelFunction("get_order_by_id")]
    [Description("Retrieve an order by positive numeric ID, including its status, customer, items, quantities, and total.")]
    public async Task<string> GetOrderById([Description("A positive order ID.")] int orderId)
    {
        if (orderId <= 0) return ToolJson.Error("Order ID must be positive.");
        var order = await orders.GetByIdAsync(orderId);
        return order is null ? ToolJson.Error($"Order {orderId} was not found.") : ToolJson.Order(order);
    }

    [KernelFunction("search_orders")]
    [Description("Search orders by order-number fragment. Returns at most 20 matching order summaries.")]
    public async Task<string> SearchOrders(
        [Description("An order number or fragment, from 2 to 50 characters.")] string orderNumber,
        [Description("Maximum number of orders, from 1 to 20.")] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(orderNumber) || orderNumber.Trim().Length is < 2 or > 50)
            return ToolJson.Error("Order number must be between 2 and 50 characters.");
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var term = orderNumber.Trim();
        var results = await orders.SearchByNumberAsync(term, limit);
        return ToolJson.Serialize(results.Select(ToolJson.OrderData));
    }

    [KernelFunction("list_recent_orders")]
    [Description("List a bounded number of recent orders, including status, customer, line items, quantities, and totals.")]
    public async Task<string> ListRecentOrders([Description("Maximum number of orders, from 1 to 20.")] int limit = 10)
    {
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var results = await orders.GetRecentAsync(limit);
        return ToolJson.Serialize(results.Select(ToolJson.OrderData));
    }

    [KernelFunction("get_highest_order_for_month")]
    [Description("Retrieve the single order with the highest total amount in the specified calendar month.")]
    public async Task<string> GetHighestOrderForMonth(
        [Description("Calendar year, from 1 to 9999.")] int year,
        [Description("Calendar month, from 1 to 12.")] int month)
    {
        if (!ToolJson.TryMonthWindow(year, month, out var start, out var end))
            return ToolJson.Error("Year or month is invalid.");
        var order = await orders.GetHighestValueBetweenAsync(start, end);
        return order is null ? ToolJson.Error("No orders were found for that month.") : ToolJson.Order(order);
    }

    [KernelFunction("get_top_orders_for_month")]
    [Description("Retrieve the top 1 to 20 orders in the specified calendar month, ranked by total amount from highest to lowest.")]
    public async Task<string> GetTopOrdersForMonth(
        [Description("Calendar year, from 1 to 9999.")] int year,
        [Description("Calendar month, from 1 to 12.")] int month,
        [Description("Number of orders to return, from 1 to 20.")] int limit = 5)
    {
        if (!ToolJson.TryMonthWindow(year, month, out var start, out var end))
            return ToolJson.Error("Year or month is invalid.");
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var results = await orders.GetTopValueBetweenAsync(start, end, limit);
        return ToolJson.Serialize(results.Select(ToolJson.OrderData));
    }

    [KernelFunction("list_pending_orders")]
    [Description("List up to 20 pending orders, including order number, customer, line items, and total.")]
    public async Task<string> ListPendingOrders([Description("Maximum number of pending orders, from 1 to 20.")] int limit = 20)
    {
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var results = await orders.GetPendingAsync(limit);
        return ToolJson.Serialize(results.Select(ToolJson.OrderData));
    }
}
