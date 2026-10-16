using AiBusinessAssistant.DTOs.Order;

namespace AiBusinessAssistant.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto> CompleteAsync(int id);
        Task DeleteAsync(int id);
    }
}
