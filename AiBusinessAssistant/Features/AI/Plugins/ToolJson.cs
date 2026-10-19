using System.Text.Json;
using AiBusinessAssistant.DTOs.Order;
using AiBusinessAssistant.DTOs.Product;

namespace AiBusinessAssistant.Features.AI.Plugins;

internal static class ToolJson
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);
    public static string Error(string message) => Serialize(new { error = message });
    public static bool ValidQuery(string? query) => !string.IsNullOrWhiteSpace(query) && query.Trim().Length is >= 2 and <= 100;
    public static bool ValidLimit(int limit, int maximum) => limit is >= 1 && limit <= maximum;

    public static bool TryMonthWindow(int year, int month, out DateTime start, out DateTime end)
    {
        start = default;
        end = default;
        if (year is < 1 or > 9999 || month is < 1 or > 12) return false;
        start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        end = start.AddMonths(1);
        return true;
    }

    public static object ProductData(ProductDto product) => new
    {
        product.ProductId,
        product.Name,
        product.SKU,
        product.UnitPrice,
        product.StockQuantity,
        product.IsActive
    };

    public static string Product(ProductDto product) => Serialize(ProductData(product));

    public static object OrderData(OrderDto order) => new
    {
        order.OrderId,
        order.OrderNumber,
        order.CustomerId,
        order.CustomerName,
        order.OrderDate,
        order.Status,
        order.TotalAmount,
        Items = order.OrderItems.Select(item => new
        {
            item.ProductId,
            item.ProductName,
            item.Quantity,
            item.UnitPrice
        })
    };

    public static string Order(OrderDto order) => Serialize(OrderData(order));
}
