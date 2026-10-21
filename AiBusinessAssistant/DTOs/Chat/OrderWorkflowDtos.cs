using AiBusinessAssistant.DTOs.Order;

namespace AiBusinessAssistant.DTOs.Chat;

public sealed class OrderWorkflowActionRequestDto
{
    public string Action { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public int? OrderId { get; set; }
    public List<OrderWorkflowItemRequestDto>? Items { get; set; }
}

public sealed class OrderWorkflowItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class OrderWorkflowResponseDto
{
    public int ConversationId { get; set; }
    public string WorkflowKind { get; set; } = "PlaceOrder";
    public string State { get; set; } = "Idle";
    public string AssistantMessage { get; set; } = string.Empty;
    public OrderWorkflowCustomerDto? SelectedCustomer { get; set; }
    public List<OrderWorkflowCustomerDto> AvailableCustomers { get; set; } = [];
    public List<OrderWorkflowProductDto> AvailableProducts { get; set; } = [];
    public List<OrderWorkflowSelectedItemDto> SelectedItems { get; set; } = [];
    public decimal OrderTotal { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public List<OrderDto> PendingOrders { get; set; } = [];
    public OrderDto? SelectedPendingOrder { get; set; }
    public OrderDto? CompletedOrder { get; set; }
}

public sealed class OrderWorkflowCustomerDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class OrderWorkflowProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}

public sealed class OrderWorkflowSelectedItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public int StockAvailable { get; set; }
    public bool IsAvailable { get; set; }
}
