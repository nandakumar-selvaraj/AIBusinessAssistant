using AiBusinessAssistant.Entities;

namespace AiBusinessAssistant.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetRecentAsync(int limit);
        Task<IEnumerable<Order>> SearchByNumberAsync(string orderNumber, int limit);
        Task<Order?> GetHighestValueBetweenAsync(DateTime startInclusive, DateTime endExclusive);
        Task<IEnumerable<Order>> GetTopValueBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit);
        Task<IEnumerable<Order>> GetPendingAsync(int limit);
        Task<Order?> GetByIdAsync(int id);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task UpdateStatusAsync(int id, string status);
        Task<bool> TryCompletePendingAsync(int id);
        Task DeleteAsync(int id);
    }
}
