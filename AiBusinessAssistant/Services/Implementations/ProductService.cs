using AiBusinessAssistant.DTOs.Product;
using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Repositories.Interfaces;
using AiBusinessAssistant.Services.Interfaces;

namespace AiBusinessAssistant.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => MapToDto(p));
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto?> GetBySkuAsync(string sku)
        {
            var product = await _productRepository.GetBySkuAsync(sku);
            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto> CreateAsync(UpsertProductDto dto)
        {
            var entity = new Product
            {
                Name = dto.Name.Trim(),
                SKU = dto.SKU.Trim(),
                Description = dto.Description,
                UnitPrice = dto.UnitPrice,
                StockQuantity = dto.StockQuantity,
                IsActive = dto.IsActive
            };

            var created = await _productRepository.AddAsync(entity);
            return MapToDto(created);
        }

        public async Task UpdateAsync(int id, UpsertProductDto dto)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null) throw new ApiNotFoundException($"Product {id} was not found.");

            existing.Name = dto.Name.Trim();
            existing.SKU = dto.SKU.Trim();
            existing.Description = dto.Description;
            existing.UnitPrice = dto.UnitPrice;
            existing.StockQuantity = dto.StockQuantity;
            existing.IsActive = dto.IsActive;

            await _productRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        private static ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                SKU = p.SKU,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
