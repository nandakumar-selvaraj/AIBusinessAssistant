using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Data;
using AiBusinessAssistant.DTOs.Chat;
using AiBusinessAssistant.DTOs.Order;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Features.AI;

public interface IOrderWorkflowService
{
    Task<OrderWorkflowResponseDto> StartAsync(int conversationId, CancellationToken cancellationToken);
    Task<OrderWorkflowResponseDto> StartPendingCompletionAsync(int conversationId, CancellationToken cancellationToken);
    Task<OrderWorkflowResponseDto> HandleActionAsync(int conversationId, OrderWorkflowActionRequestDto request, CancellationToken cancellationToken);
    Task<OrderWorkflowResponseDto?> GetActiveAsync(int conversationId, CancellationToken cancellationToken);
}

public sealed class OrderWorkflowService(
    BusinessAiDbContext db,
    ICustomerService customers,
    IProductService products,
    IOrderService orders,
    ConversationLockProvider locks) : IOrderWorkflowService
{
    private static readonly SemaphoreSlim OrderNumberLock = new(1, 1);
    private const string OrderNumberPrefix = "ORD-SAMPLE-";
    private const string Idle = "Idle";
    private const string SelectingCustomer = "SelectingCustomer";
    private const string SelectingProducts = "SelectingProducts";
    private const string AwaitingConfirmation = "AwaitingConfirmation";
    private const string ListingPendingOrders = "ListingPendingOrders";
    private const string ReviewingPendingOrders = "ReviewingPendingOrders";
    private const string AwaitingCompletionConfirmation = "AwaitingCompletionConfirmation";
    private const string Completed = "Completed";
    private const string Cancelled = "Cancelled";

    public async Task<OrderWorkflowResponseDto> StartAsync(int conversationId, CancellationToken cancellationToken)
    {
        var conversationExists = await db.ChatConversations.AnyAsync(item => item.ConversationId == conversationId, cancellationToken);
        if (!conversationExists) throw new ApiNotFoundException($"Conversation {conversationId} was not found.");

        var workflow = await LoadAsync(conversationId, cancellationToken);
        if (workflow is not null && IsPendingWorkflowActive(workflow))
            return await BuildPendingResponseAsync(workflow, "Finish reviewing pending orders before starting another workflow.", cancellationToken);
        if (workflow is null)
        {
            workflow = new OrderWorkflow { ConversationId = conversationId };
            db.OrderWorkflows.Add(workflow);
        }
        else if (workflow.State is not (SelectingCustomer or SelectingProducts or AwaitingConfirmation))
        {
            workflow.CustomerId = null;
            workflow.CreatedOrderId = null;
            workflow.State = SelectingCustomer;
            workflow.OrderNumber = await CreateOrderNumberAsync(conversationId, cancellationToken);
            await ClearItemsAsync(workflow, cancellationToken);
        }

        if (workflow.State == Idle)
        {
            workflow.State = SelectingCustomer;
            workflow.OrderNumber = await CreateOrderNumberAsync(conversationId, cancellationToken);
        }
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(workflow, "Choose a customer to start the order.", cancellationToken);
    }

    public async Task<OrderWorkflowResponseDto> StartPendingCompletionAsync(int conversationId, CancellationToken cancellationToken)
    {
        if (!await db.ChatConversations.AnyAsync(item => item.ConversationId == conversationId, cancellationToken))
            throw new ApiNotFoundException($"Conversation {conversationId} was not found.");

        var workflow = await LoadAsync(conversationId, cancellationToken);
        if (workflow is not null && workflow.State is SelectingCustomer or SelectingProducts or AwaitingConfirmation)
            return await BuildResponseAsync(workflow, "Finish the current order before reviewing pending orders.", cancellationToken);
        if (workflow is not null && workflow.State == AwaitingCompletionConfirmation)
            return await BuildPendingResponseAsync(workflow, "Confirm or cancel the selected order before choosing another.", cancellationToken);

        if (workflow is null)
        {
            workflow = new OrderWorkflow { ConversationId = conversationId };
            db.OrderWorkflows.Add(workflow);
        }
        else
        {
            workflow.CustomerId = null;
            workflow.CreatedOrderId = null;
            workflow.SelectedOrderId = null;
            await ClearItemsAsync(workflow, cancellationToken);
        }

        workflow.State = ListingPendingOrders;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        workflow.State = ReviewingPendingOrders;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        var pending = await orders.GetPendingAsync(20);
        var message = pending.Any() ? "Choose a pending order ID to review its details." : "There are no pending orders right now.";
        return await BuildPendingResponseAsync(workflow, message, cancellationToken, pending);
    }

    public async Task<OrderWorkflowResponseDto> HandleActionAsync(int conversationId, OrderWorkflowActionRequestDto request, CancellationToken cancellationToken)
    {
        await using var conversationLock = await locks.AcquireAsync(conversationId, cancellationToken);
        if (!await db.ChatConversations.AnyAsync(item => item.ConversationId == conversationId, cancellationToken))
            throw new ApiNotFoundException($"Conversation {conversationId} was not found.");

        var workflow = await LoadAsync(conversationId, cancellationToken)
            ?? throw new ApiConflictException("No order workflow exists for this conversation. Start an order first.");
        if (string.IsNullOrWhiteSpace(request.Action))
            throw new ApiValidationException("An order workflow action is required.");

        switch (request.Action.Trim().ToLowerInvariant())
        {
            case "startplaceorder":
            case "start":
                return await StartAsync(conversationId, cancellationToken);
            case "showpendingorders":
                return await StartPendingCompletionAsync(conversationId, cancellationToken);
            case "selectpendingorder":
                return await SelectPendingOrderAsync(workflow, request.OrderId, cancellationToken);
            case "confirmcompletion":
                return await ConfirmPendingOrderAsync(workflow, request.OrderId, cancellationToken);
            case "selectcustomer":
                await SelectCustomerAsync(workflow, request.CustomerId, cancellationToken);
                return await BuildResponseAsync(workflow, "Customer selected. Choose products and quantities.", cancellationToken);
            case "setorderitems":
                await SetItemsAsync(workflow, request.Items, cancellationToken);
                return await BuildResponseAsync(workflow, "Review the order summary, then confirm or cancel.", cancellationToken);
            case "editorderitems":
                EnsureState(workflow, AwaitingConfirmation);
                workflow.State = SelectingProducts;
                workflow.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                return await BuildResponseAsync(workflow, "Adjust the products and quantities, then review the order again.", cancellationToken);
            case "confirmorder":
                return await ConfirmAsync(workflow, cancellationToken);
            case "cancelworkflow":
                if (workflow.State == AwaitingCompletionConfirmation)
                {
                    workflow.State = Cancelled;
                    workflow.SelectedOrderId = null;
                    workflow.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(cancellationToken);
                    return await BuildPendingResponseAsync(workflow, "Completion cancelled. The order was not changed.", cancellationToken);
                }
                EnsureState(workflow, SelectingCustomer, SelectingProducts, AwaitingConfirmation);
                workflow.State = Cancelled;
                workflow.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                return await BuildResponseAsync(workflow, "Order cancelled. No order was created.", cancellationToken);
            default:
                throw new ApiValidationException("Unsupported order workflow action.");
        }
    }

    public async Task<OrderWorkflowResponseDto?> GetActiveAsync(int conversationId, CancellationToken cancellationToken)
    {
        if (!await db.ChatConversations.AnyAsync(item => item.ConversationId == conversationId, cancellationToken))
            throw new ApiNotFoundException($"Conversation {conversationId} was not found.");
        var workflow = await LoadAsync(conversationId, cancellationToken);
        if (workflow is null || workflow.State is Idle or Completed or Cancelled) return null;
        if (IsPendingWorkflowActive(workflow))
            return await BuildPendingResponseAsync(workflow, "Continue reviewing pending orders below.", cancellationToken);
        return await BuildResponseAsync(workflow, "Continue using the order controls below.", cancellationToken);
    }

    private async Task SelectCustomerAsync(OrderWorkflow workflow, int? customerId, CancellationToken cancellationToken)
    {
        EnsureState(workflow, SelectingCustomer);
        if (!customerId.HasValue || customerId.Value <= 0)
            throw new ApiValidationException("Choose a valid customer.");
        var customer = await customers.GetByIdAsync(customerId.Value);
        if (customer is null) throw new ApiNotFoundException($"Customer {customerId.Value} was not found.");
        workflow.CustomerId = customer.CustomerId;
        workflow.State = SelectingProducts;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SetItemsAsync(OrderWorkflow workflow, List<OrderWorkflowItemRequestDto>? requestedItems, CancellationToken cancellationToken)
    {
        EnsureState(workflow, SelectingProducts, AwaitingConfirmation);
        if (workflow.CustomerId is null) throw new ApiConflictException("Select a customer before choosing products.");
        if (requestedItems is null || requestedItems.Count == 0)
            throw new ApiValidationException("Choose at least one product and enter a quantity.");
        if (requestedItems.Any(item => item.ProductId <= 0 || item.Quantity <= 0))
            throw new ApiValidationException("Product IDs and quantities must be positive.");
        if (requestedItems.Select(item => item.ProductId).Distinct().Count() != requestedItems.Count)
            throw new ApiValidationException("Each product can appear only once in the order.");

        var catalog = (await products.GetAllAsync()).ToDictionary(product => product.ProductId);
        foreach (var item in requestedItems)
        {
            if (!catalog.TryGetValue(item.ProductId, out var product))
                throw new ApiNotFoundException($"Product {item.ProductId} was not found.");
            if (!product.IsActive) throw new ApiConflictException($"'{product.Name}' is inactive and cannot be ordered.");
            if (item.Quantity > product.StockQuantity)
                throw new ApiConflictException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}.");
        }

        var requestedById = requestedItems.ToDictionary(item => item.ProductId);
        var removedItems = workflow.Items.Where(item => !requestedById.ContainsKey(item.ProductId)).ToList();
        db.OrderWorkflowItems.RemoveRange(removedItems);
        foreach (var removed in removedItems) workflow.Items.Remove(removed);
        foreach (var item in requestedItems)
        {
            var existing = workflow.Items.FirstOrDefault(current => current.ProductId == item.ProductId);
            if (existing is null)
            {
                workflow.Items.Add(new OrderWorkflowItem
                {
                    ConversationId = workflow.ConversationId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = catalog[item.ProductId].UnitPrice
                });
            }
            else
            {
                existing.Quantity = item.Quantity;
                existing.UnitPrice = catalog[item.ProductId].UnitPrice;
            }
        }
        workflow.State = AwaitingConfirmation;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<OrderWorkflowResponseDto> ConfirmAsync(OrderWorkflow workflow, CancellationToken cancellationToken)
    {
        if (workflow.State == Completed && workflow.CreatedOrderId.HasValue)
            return await BuildResponseAsync(workflow, "This order was already placed.", cancellationToken);
        EnsureState(workflow, AwaitingConfirmation);
        if (!workflow.CustomerId.HasValue || workflow.Items.Count == 0)
            throw new ApiConflictException("The pending order is incomplete. Select a customer and at least one product.");

        var catalog = (await products.GetAllAsync()).ToDictionary(product => product.ProductId);
        var pricesChanged = false;
        foreach (var item in workflow.Items)
        {
            if (!catalog.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                throw new ApiConflictException("A selected product is no longer available. Update the order before confirming.");
            if (item.Quantity > product.StockQuantity)
                throw new ApiConflictException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}. Update the order before confirming.");
            if (item.UnitPrice != product.UnitPrice)
            {
                item.UnitPrice = product.UnitPrice;
                pricesChanged = true;
            }
        }
        if (pricesChanged)
        {
            workflow.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return await BuildResponseAsync(workflow, "A product price changed. Review the updated total and confirm again.", cancellationToken);
        }

        var existingOrderId = await db.Orders.AsNoTracking()
            .Where(order => order.OrderNumber == workflow.OrderNumber)
            .Select(order => (int?)order.OrderId)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingOrderId.HasValue)
        {
            workflow.CreatedOrderId = existingOrderId.Value;
            workflow.State = Completed;
            await db.SaveChangesAsync(cancellationToken);
            return await BuildResponseAsync(workflow, "Order placed successfully.", cancellationToken);
        }

        var order = await orders.CreateAsync(new CreateOrderDto
        {
            OrderNumber = workflow.OrderNumber,
            CustomerId = workflow.CustomerId.Value,
            OrderItems = workflow.Items.Select(item => new CreateOrderItemDto { ProductId = item.ProductId, Quantity = item.Quantity }).ToList()
        });
        workflow.CreatedOrderId = order.OrderId;
        workflow.State = Completed;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(workflow, $"Order placed successfully. Order #{order.OrderId}.", cancellationToken);
    }

    private async Task<OrderWorkflowResponseDto> SelectPendingOrderAsync(OrderWorkflow workflow, int? orderId, CancellationToken cancellationToken)
    {
        EnsureState(workflow, ReviewingPendingOrders, Completed);
        if (!orderId.HasValue || orderId.Value <= 0)
            throw new ApiValidationException("Choose a valid pending order ID.");

        var order = await orders.GetByIdAsync(orderId.Value);
        if (order is null || !string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            workflow.SelectedOrderId = null;
            workflow.State = ReviewingPendingOrders;
            workflow.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            var message = order is null
                ? $"Order {orderId.Value} was not found. The pending list has been refreshed."
                : $"Order {orderId.Value} is no longer pending. The pending list has been refreshed.";
            return await BuildPendingResponseAsync(workflow, message, cancellationToken);
        }

        workflow.SelectedOrderId = order.OrderId;
        workflow.State = AwaitingCompletionConfirmation;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await BuildPendingResponseAsync(workflow, $"Review order {order.OrderId}, then confirm completion or cancel.", cancellationToken);
    }

    private async Task<OrderWorkflowResponseDto> ConfirmPendingOrderAsync(OrderWorkflow workflow, int? orderId, CancellationToken cancellationToken)
    {
        if (workflow.State == Completed && workflow.SelectedOrderId.HasValue && orderId == workflow.SelectedOrderId)
            return await BuildPendingResponseAsync(workflow, $"Order {workflow.SelectedOrderId.Value} is already completed.", cancellationToken);

        EnsureState(workflow, AwaitingCompletionConfirmation);
        if (!workflow.SelectedOrderId.HasValue || !orderId.HasValue || workflow.SelectedOrderId.Value != orderId.Value)
            throw new ApiConflictException("The confirmation does not match the order selected in this conversation.");

        var currentOrder = await orders.GetByIdAsync(workflow.SelectedOrderId.Value);
        if (currentOrder is null || !string.Equals(currentOrder.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            workflow.SelectedOrderId = null;
            workflow.State = ReviewingPendingOrders;
            workflow.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return await BuildPendingResponseAsync(workflow,
                currentOrder is null ? "The selected order no longer exists. The pending list has been refreshed." : "The selected order is no longer pending. The pending list has been refreshed.",
                cancellationToken);
        }

        var completedOrder = await orders.CompletePendingAsync(workflow.SelectedOrderId.Value);
        if (completedOrder is null)
        {
            workflow.SelectedOrderId = null;
            workflow.State = ReviewingPendingOrders;
            workflow.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return await BuildPendingResponseAsync(workflow, "The order changed before it could be completed. The pending list has been refreshed.", cancellationToken);
        }

        workflow.State = Completed;
        workflow.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        var remaining = await orders.GetPendingAsync(20);
        var message = remaining.Any()
            ? $"Order {completedOrder.OrderId} is now {completedOrder.Status}. Select another pending order ID if needed."
            : $"Order {completedOrder.OrderId} is now {completedOrder.Status}. No pending orders remain.";
        return await BuildPendingResponseAsync(workflow, message, cancellationToken, remaining, completedOrder);
    }

    private async Task<OrderWorkflowResponseDto> BuildPendingResponseAsync(
        OrderWorkflow workflow,
        string message,
        CancellationToken cancellationToken,
        IEnumerable<OrderDto>? pendingOrders = null,
        OrderDto? completedOrder = null)
    {
        var orderList = (pendingOrders ?? await orders.GetPendingAsync(20)).ToList();
        OrderDto? selectedOrder = null;
        if (workflow.SelectedOrderId.HasValue)
        {
            selectedOrder = await orders.GetByIdAsync(workflow.SelectedOrderId.Value);
            if (workflow.State == Completed)
            {
                completedOrder ??= selectedOrder;
                selectedOrder = null;
            }
        }

        return new OrderWorkflowResponseDto
        {
            ConversationId = workflow.ConversationId,
            WorkflowKind = "PendingOrderCompletion",
            State = workflow.State,
            AssistantMessage = message,
            PendingOrders = orderList.ToList(),
            SelectedPendingOrder = selectedOrder,
            CompletedOrder = completedOrder,
            AvailableActions = workflow.State switch
            {
                ListingPendingOrders => ["ShowPendingOrders"],
                ReviewingPendingOrders => ["SelectPendingOrder", "ShowPendingOrders"],
                AwaitingCompletionConfirmation => ["ConfirmCompletion", "CancelWorkflow"],
                Completed => ["SelectPendingOrder", "ShowPendingOrders"],
                Cancelled => ["ShowPendingOrders"],
                _ => []
            }
        };
    }

    private async Task<OrderWorkflowResponseDto> BuildResponseAsync(OrderWorkflow workflow, string message, CancellationToken cancellationToken)
    {
        var persistedOrder = workflow.CreatedOrderId.HasValue
            ? await orders.GetByIdAsync(workflow.CreatedOrderId.Value)
            : null;
        var allCustomers = await customers.GetAllAsync();
        var customer = workflow.CustomerId.HasValue
            ? allCustomers.FirstOrDefault(item => item.CustomerId == workflow.CustomerId.Value)
            : null;
        var catalog = (await products.GetAllAsync()).ToDictionary(item => item.ProductId);
        var selectedItems = workflow.Items.Select(item =>
        {
            var hasProduct = catalog.TryGetValue(item.ProductId, out var product);
            var unitPrice = workflow.State == Completed || !hasProduct ? item.UnitPrice : product!.UnitPrice;
            return new OrderWorkflowSelectedItemDto
            {
                ProductId = item.ProductId,
                Name = hasProduct ? product!.Name : $"Unavailable product #{item.ProductId}",
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                LineTotal = item.Quantity * unitPrice,
                StockAvailable = hasProduct ? product!.StockQuantity : 0,
                IsAvailable = hasProduct && product!.IsActive
            };
        }).ToList();
        var result = new OrderWorkflowResponseDto
        {
            ConversationId = workflow.ConversationId,
            State = workflow.State,
            AssistantMessage = message,
            SelectedCustomer = customer is null ? null : new OrderWorkflowCustomerDto { CustomerId = customer.CustomerId, Name = customer.Name },
            SelectedItems = selectedItems,
            OrderTotal = persistedOrder?.TotalAmount ?? selectedItems.Sum(item => item.LineTotal),
            OrderId = workflow.CreatedOrderId
        };

        if (workflow.State == SelectingCustomer)
            result.AvailableCustomers = allCustomers.Select(item => new OrderWorkflowCustomerDto { CustomerId = item.CustomerId, Name = item.Name }).ToList();
        if (workflow.State is SelectingProducts or AwaitingConfirmation)
            result.AvailableProducts = catalog.Values
                .Where(item => item.IsActive && item.StockQuantity > 0)
                .OrderBy(item => item.Name)
                .Select(item => new OrderWorkflowProductDto
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Sku = item.SKU,
                    UnitPrice = item.UnitPrice,
                    StockQuantity = item.StockQuantity
                }).ToList();

        result.AvailableActions = workflow.State switch
        {
            SelectingCustomer => ["SelectCustomer", "CancelWorkflow"],
            SelectingProducts => ["SetOrderItems", "CancelWorkflow"],
            AwaitingConfirmation => ["EditOrderItems", "ConfirmOrder", "CancelWorkflow"],
            Completed => ["StartPlaceOrder"],
            Cancelled => ["StartPlaceOrder"],
            _ => []
        };
        if (workflow.CreatedOrderId.HasValue)
        {
            result.OrderNumber = persistedOrder?.OrderNumber;
        }
        return result;
    }

    private Task<OrderWorkflow?> LoadAsync(int conversationId, CancellationToken cancellationToken) =>
        db.OrderWorkflows.Include(workflow => workflow.Items)
            .FirstOrDefaultAsync(workflow => workflow.ConversationId == conversationId, cancellationToken);

    private async Task ClearItemsAsync(OrderWorkflow workflow, CancellationToken cancellationToken)
    {
        foreach (var item in workflow.Items.ToList()) db.Entry(item).State = EntityState.Detached;
        await db.OrderWorkflowItems
            .Where(item => item.ConversationId == workflow.ConversationId)
            .ExecuteDeleteAsync(cancellationToken);
        workflow.Items.Clear();
    }

    private static void EnsureState(OrderWorkflow workflow, params string[] allowed)
    {
        if (!allowed.Contains(workflow.State, StringComparer.Ordinal))
            throw new ApiConflictException($"This action is not valid while the order workflow is {workflow.State}.");
    }

    private static bool IsPendingWorkflowActive(OrderWorkflow workflow) =>
        workflow.State is ListingPendingOrders or ReviewingPendingOrders or AwaitingCompletionConfirmation;

    private async Task<string> CreateOrderNumberAsync(int conversationId, CancellationToken cancellationToken)
    {
        await OrderNumberLock.WaitAsync(cancellationToken);
        try
        {
            var orderNumbers = await db.Orders.AsNoTracking()
                .Where(order => order.OrderNumber.StartsWith(OrderNumberPrefix))
                .Select(order => order.OrderNumber)
                .ToListAsync(cancellationToken);
            var pendingNumbers = await db.OrderWorkflows.AsNoTracking()
                .Where(workflow => workflow.ConversationId != conversationId &&
                    (workflow.State == SelectingCustomer || workflow.State == SelectingProducts || workflow.State == AwaitingConfirmation))
                .Select(workflow => workflow.OrderNumber)
                .ToListAsync(cancellationToken);

            var highest = orderNumbers.Concat(pendingNumbers)
                .Select(ParseOrderSequence)
                .DefaultIfEmpty(0)
                .Max();
            return $"{OrderNumberPrefix}{checked(highest + 1):D3}";
        }
        finally
        {
            OrderNumberLock.Release();
        }
    }

    private static int ParseOrderSequence(string orderNumber)
    {
        var suffix = orderNumber.AsSpan(OrderNumberPrefix.Length);
        return int.TryParse(suffix, out var sequence) && sequence > 0 ? sequence : 0;
    }
}
