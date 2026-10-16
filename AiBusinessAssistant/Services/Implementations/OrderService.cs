using AiBusinessAssistant.DTOs.Order;
using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Repositories.Interfaces;
using AiBusinessAssistant.Services.Interfaces;

namespace AiBusinessAssistant.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o));
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OrderNumber))
                throw new ApiValidationException("Order number is required.");

            var orderNumber = dto.OrderNumber.Trim();
            if (orderNumber.Length > 50)
                throw new ApiValidationException("Order number cannot exceed 50 characters.");

            if (dto.CustomerId <= 0)
                throw new ApiValidationException("Customer ID must be a positive number.");

            if (dto.OrderItems is null || dto.OrderItems.Count == 0)
                throw new ApiValidationException("An order must contain at least one item.");

            if (dto.OrderItems.Any(item => item.ProductId <= 0 || item.Quantity <= 0))
                throw new ApiValidationException("Every order item must have a valid product and a quantity greater than zero.");

            if (await _customerRepository.GetByIdAsync(dto.CustomerId) is null)
                throw new ApiNotFoundException($"Customer {dto.CustomerId} was not found.");

            foreach (var productId in dto.OrderItems.Select(item => item.ProductId).Distinct())
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product is null)
                    throw new ApiNotFoundException($"Product {productId} was not found.");
                if (!product.IsActive)
                    throw new ApiConflictException($"Product {productId} is inactive and cannot be ordered.");
            }

            var entity = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = dto.CustomerId,
                Status = "Pending",
                OrderItems = dto.OrderItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var created = await _orderRepository.AddAsync(entity);
            return MapToDto(created);
        }

        public async Task<OrderDto> CompleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new ApiNotFoundException($"Order {id} was not found.");

            if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                return MapToDto(order);

            if (!string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(order.Status, "Processing", StringComparison.OrdinalIgnoreCase))
                throw new ApiConflictException($"Order {id} cannot be completed from status '{order.Status}'.");

            await _orderRepository.UpdateStatusAsync(id, "Completed");
            order.Status = "Completed";
            return MapToDto(order);
        }

        public async Task DeleteAsync(int id) => await _orderRepository.DeleteAsync(id);

        private static OrderDto MapToDto(Order o)
        {
            return new OrderDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.Name ?? string.Empty,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}
