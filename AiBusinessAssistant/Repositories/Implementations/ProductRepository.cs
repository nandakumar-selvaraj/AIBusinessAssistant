using AiBusinessAssistant.Data;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly BusinessAiDbContext _context;

        public ProductRepository(BusinessAiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync() =>
            await _context.Products.ToListAsync();

        public async Task<IEnumerable<Product>> GetTopAsync(int limit) =>
            await _context.Products.AsNoTracking().OrderBy(p => p.Name).Take(limit).ToListAsync();

        public async Task<IEnumerable<Product>> SearchAsync(string query, int limit) =>
            await _context.Products.AsNoTracking()
                .Where(p => EF.Functions.Like(p.Name, $"%{EscapeLike(query)}%", "\\") ||
                    EF.Functions.Like(p.SKU, $"%{EscapeLike(query)}%", "\\"))
                .OrderBy(p => p.Name).Take(limit).ToListAsync();

        public async Task<IEnumerable<Product>> GetActiveLowStockAsync(int stockThreshold, int limit) =>
            await _context.Products.AsNoTracking()
                .Where(product => product.IsActive && product.StockQuantity <= stockThreshold)
                .OrderBy(product => product.StockQuantity).ThenBy(product => product.Name)
                .Take(limit).ToListAsync();

        private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.AsNoTracking().FirstOrDefaultAsync(product => product.ProductId == id);

        public async Task<Product?> GetBySkuAsync(string sku) =>
            await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);

        public async Task<Product> AddAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
                throw new ApiNotFoundException($"Product {id} was not found.");

            if (await _context.OrderItems.AnyAsync(item => item.ProductId == id))
                throw new ApiConflictException("This product is referenced by order items and cannot be deleted. Deactivate it to preserve order history.");

            _context.Products.Remove(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ApiConflictException("This product is referenced by an order item and cannot be deleted.");
            }
        }
    }
}
