using AiBusinessAssistant.DTOs.Order;

namespace AiBusinessAssistant.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<IEnumerable<OrderDto>> GetRecentAsync(int limit);
        Task<IEnumerable<OrderDto>> SearchByNumberAsync(string orderNumber, int limit);
        Task<OrderDto?> GetHighestValueBetweenAsync(DateTime startInclusive, DateTime endExclusive);
        Task<IEnumerable<OrderDto>> GetTopValueBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit);
        Task<IEnumerable<OrderDto>> GetPendingAsync(int limit);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto> CompleteAsync(int id);
        Task<OrderDto?> CompletePendingAsync(int id);
        Task DeleteAsync(int id);
    }
}
