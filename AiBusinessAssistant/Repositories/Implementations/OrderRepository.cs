using AiBusinessAssistant.Data;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BusinessAiDbContext _context;

        public OrderRepository(BusinessAiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync() =>
            await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetRecentAsync(int limit) =>
            await _context.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(item => item.Product)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId)
                .Take(limit).ToListAsync();

        public async Task<IEnumerable<Order>> SearchByNumberAsync(string orderNumber, int limit) =>
            await _context.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(item => item.Product)
                .Where(o => EF.Functions.Like(o.OrderNumber, $"%{EscapeLike(orderNumber)}%", "\\"))
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId)
                .Take(limit).ToListAsync();

        public async Task<Order?> GetHighestValueBetweenAsync(DateTime startInclusive, DateTime endExclusive) =>
            await OrderQuery()
                .Where(order => order.OrderDate >= startInclusive && order.OrderDate < endExclusive)
                .OrderByDescending(order => order.TotalAmount).ThenByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.OrderId).FirstOrDefaultAsync();

        public async Task<IEnumerable<Order>> GetTopValueBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit) =>
            await OrderQuery()
                .Where(order => order.OrderDate >= startInclusive && order.OrderDate < endExclusive)
                .OrderByDescending(order => order.TotalAmount).ThenByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.OrderId).Take(limit).ToListAsync();

        public async Task<IEnumerable<Order>> GetPendingAsync(int limit) =>
            await OrderQuery().Where(order => order.Status == "Pending")
                .OrderByDescending(order => order.OrderDate).ThenByDescending(order => order.OrderId)
                .Take(limit).ToListAsync();

        private IQueryable<Order> OrderQuery() => _context.Orders.AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.OrderItems).ThenInclude(item => item.Product);

        private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        public async Task<Order?> GetByIdAsync(int id) =>
            await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

        public async Task<Order> AddAsync(Order order)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId);
            if (customer is null)
                throw new ApiNotFoundException($"Customer {order.CustomerId} was not found.");

            var productIds = order.OrderItems.Select(item => item.ProductId).Distinct().ToArray();
            var products = await _context.Products
                .AsNoTracking()
                .Where(product => productIds.Contains(product.ProductId))
                .ToDictionaryAsync(product => product.ProductId);
            var missingProductId = productIds.FirstOrDefault(productId => !products.ContainsKey(productId));
            if (missingProductId != 0)
                throw new ApiNotFoundException($"Product {missingProductId} was not found.");

            if (products.Values.Any(product => !product.IsActive))
                throw new ApiConflictException("Inactive products cannot be added to an order.");

            var requestedQuantities = order.OrderItems
                .GroupBy(item => item.ProductId)
                .ToDictionary(group => group.Key, group => group.Sum(item => (long)item.Quantity));

            if (await _context.Orders.AnyAsync(existing => existing.OrderNumber == order.OrderNumber))
                throw new ApiConflictException($"Order number '{order.OrderNumber}' already exists.");

            foreach (var product in products.Values)
                _context.Products.Attach(product);

            foreach (var (productId, requestedQuantity) in requestedQuantities)
            {
                var product = products[productId];
                if (requestedQuantity > int.MaxValue)
                    throw new ApiConflictException($"Insufficient stock for '{product.Name}'. Requested {requestedQuantity}, available {product.StockQuantity}.");

                var quantityToReserve = (int)requestedQuantity;
                var updatedRows = await _context.Products
                    .Where(candidate => candidate.ProductId == productId && candidate.IsActive && candidate.StockQuantity >= quantityToReserve)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(candidate => candidate.StockQuantity, candidate => candidate.StockQuantity - quantityToReserve));

                if (updatedRows == 0)
                {
                    var currentProduct = await _context.Products.AsNoTracking()
                        .FirstOrDefaultAsync(candidate => candidate.ProductId == productId);
                    if (currentProduct is null)
                        throw new ApiNotFoundException($"Product {productId} was not found.");
                    if (!currentProduct.IsActive)
                        throw new ApiConflictException($"Product '{currentProduct.Name}' is inactive and cannot be ordered.");
                    throw new ApiConflictException($"Insufficient stock for '{currentProduct.Name}'. Requested {requestedQuantity}, available {currentProduct.StockQuantity}.");
                }
            }

            var now = DateTime.UtcNow;
            order.CreatedAt = now;
            order.OrderDate = now;
            order.Customer = customer;
            foreach (var item in order.OrderItems)
            {
                if (item.ProductId <= 0 || item.Quantity <= 0)
                    throw new ApiValidationException("Every order item must have a valid product and a quantity greater than zero.");
                item.Product = products[item.ProductId];
                item.UnitPrice = item.Product.UnitPrice;
            }

            try
            {
                order.TotalAmount = order.OrderItems.Sum(item => checked(item.Quantity * item.UnitPrice));
            }
            catch (OverflowException)
            {
                throw new ApiValidationException("The order total exceeds the supported amount.");
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(existing => existing.OrderId == id);
            if (order is null)
                throw new ApiNotFoundException($"Order {id} was not found.");

            order.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order is null)
                throw new ApiNotFoundException($"Order {id} was not found.");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
