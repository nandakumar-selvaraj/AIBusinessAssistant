using AiBusinessAssistant.Entities;

namespace AiBusinessAssistant.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetTopAsync(int limit);
        Task<IEnumerable<Product>> SearchAsync(string query, int limit);
        Task<IEnumerable<Product>> GetActiveLowStockAsync(int stockThreshold, int limit);
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetBySkuAsync(string sku);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}
