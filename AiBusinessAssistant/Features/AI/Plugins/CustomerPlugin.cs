using System.ComponentModel;
using System.Text.Json;
using AiBusinessAssistant.Services.Interfaces;
using Microsoft.SemanticKernel;

namespace AiBusinessAssistant.Features.AI.Plugins;

public sealed class CustomerPlugin(ICustomerService customers)
{
    private const int MaximumResults = 20;

    [KernelFunction("get_customer_by_id")]
    [Description("Retrieve basic, non-sensitive customer information by numeric customer ID.")]
    public async Task<string> GetCustomerById(
        [Description("A positive customer ID.")] int customerId)
    {
        if (customerId <= 0) return ToolJson.Error("Customer ID must be positive.");
        var customer = await customers.GetByIdAsync(customerId);
        return customer is null
            ? ToolJson.Error($"Customer {customerId} was not found.")
            : ToolJson.Serialize(new { customer.CustomerId, customer.Name, customer.Email });
    }

    [KernelFunction("search_customers")]
    [Description("Search customers by name or email. Returns at most 20 matches and excludes phone and address.")]
    public async Task<string> SearchCustomers(
        [Description("A name or email fragment, from 2 to 100 characters.")] string query,
        [Description("Maximum number of matches, from 1 to 20.")] int limit = 10)
    {
        if (!ToolJson.ValidQuery(query)) return ToolJson.Error("Search text must be between 2 and 100 characters.");
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var term = query.Trim();
        var results = (await customers.SearchAsync(term, limit))
            .Select(customer => new { customer.CustomerId, customer.Name, customer.Email });
        return ToolJson.Serialize(results);
    }

    [KernelFunction("list_customers")]
    [Description("List a bounded number of customer names and email addresses. Phone numbers and addresses are not returned.")]
    public async Task<string> ListCustomers(
        [Description("Maximum number of customers, from 1 to 20.")] int limit = 10)
    {
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var results = (await customers.GetTopAsync(limit))
            .Select(customer => new { customer.CustomerId, customer.Name, customer.Email });
        return ToolJson.Serialize(results);
    }

    [KernelFunction("get_registered_customers_for_month")]
    [Description("List up to 20 customers registered in a specified calendar month. Only basic names and email addresses are returned.")]
    public async Task<string> GetRegisteredCustomersForMonth(
        [Description("Calendar year, from 1 to 9999.")] int year,
        [Description("Calendar month, from 1 to 12.")] int month,
        [Description("Maximum number of customers, from 1 to 20.")] int limit = 20)
    {
        if (!ToolJson.TryMonthWindow(year, month, out var start, out var end))
            return ToolJson.Error("Year or month is invalid.");
        if (!ToolJson.ValidLimit(limit, MaximumResults)) return ToolJson.Error("Limit must be between 1 and 20.");
        var customersInMonth = await customers.GetRegisteredBetweenAsync(start, end, limit);
        return ToolJson.Serialize(customersInMonth.Select(customer => new
        {
            customer.CustomerId,
            customer.Name,
            customer.Email,
            customer.CreatedAt
        }));
    }
}
