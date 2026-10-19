using AiBusinessAssistant.DTOs.Product;

namespace AiBusinessAssistant.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetTopAsync(int limit);
        Task<IEnumerable<ProductDto>> SearchAsync(string query, int limit);
        Task<IEnumerable<ProductDto>> GetActiveLowStockAsync(int stockThreshold, int limit);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto?> GetBySkuAsync(string sku);
        Task<ProductDto> CreateAsync(UpsertProductDto dto);
        Task UpdateAsync(int id, UpsertProductDto dto);
        Task DeleteAsync(int id);
    }
}
