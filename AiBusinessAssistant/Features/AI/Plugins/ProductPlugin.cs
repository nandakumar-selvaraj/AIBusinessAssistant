using System.ComponentModel;
using AiBusinessAssistant.Services.Interfaces;
using Microsoft.SemanticKernel;

namespace AiBusinessAssistant.Features.AI.Plugins;

public sealed class ProductPlugin(IProductService products)
{
    private const int MaximumResults = 20;

    [KernelFunction("get_product_by_id")]
    [Description("Retrieve product name, SKU, price, current stock, and active status by ID.")]
    public async Task<string> GetProductById([Description("A positive product ID.")] int productId)
    {
        if (productId <= 0) return ToolJson.Error("Product ID must be positive.");
        var product = await products.GetByIdAsync(productId);
        return product is null ? ToolJson.Error($"Product {productId} was not found.") : ToolJson.Product(product);
    }

    [KernelFunction("get_product_by_sku")]
    [Description("Retrieve product name, SKU, price, current stock, and active status by exact SKU.")]
    public async Task<string> GetProductBySku([Description("A SKU between 1 and 100 characters.")] string sku)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 100)
            return ToolJson.Error("SKU must be between 1 and 100 characters.");
        var product = await products.GetBySkuAsync(sku.Trim());
        return product is null ? ToolJson.Error($"Product with SKU '{sku.Trim()}' was not found.") : ToolJson.Product(product);
    }

    [KernelFunction("search_products")]
    [Description("Search products by name or SKU. Returns at most 20 results with actual stored price, stock, and active status.")]
    public async Task<string> SearchProducts(
        [Description("A product name or SKU fragment, from 2 to 100 characters.")] string query,
        [Description("Maximum number of products, from 1 to 20.")] int limit = 10)
    {
        if (!ToolJson.ValidQuery(query)) return ToolJson.Error("Search text must be between 2 and 100 characters.");
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var term = query.Trim();
        var results = await products.SearchAsync(term, limit);
        return ToolJson.Serialize(results.Select(ToolJson.ProductData));
    }

    [KernelFunction("list_products")]
    [Description("List a bounded number of products and their stored price, stock, and active status.")]
    public async Task<string> ListProducts([Description("Maximum number of products, from 1 to 20.")] int limit = 10)
    {
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var results = await products.GetTopAsync(limit);
        return ToolJson.Serialize(results.Select(ToolJson.ProductData));
    }

    [KernelFunction("list_active_low_stock_products")]
    [Description("List active products with stock at or below the requested threshold. Returns up to 20 actual product records.")]
    public async Task<string> ListActiveLowStockProducts(
        [Description("Nonnegative stock threshold, up to 100000.")] int stockThreshold = 5,
        [Description("Maximum number of products, from 1 to 20.")] int limit = 20)
    {
        if (stockThreshold is < 0 or > 100000) return ToolJson.Error("Stock threshold must be between 0 and 100000.");
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var results = await products.GetActiveLowStockAsync(stockThreshold, limit);
        return ToolJson.Serialize(results.Select(ToolJson.ProductData));
    }
}
