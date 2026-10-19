using AiBusinessAssistant.Entities;

namespace AiBusinessAssistant.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<IEnumerable<Customer>> GetTopAsync(int limit);
        Task<IEnumerable<Customer>> SearchAsync(string query, int limit);
        Task<IEnumerable<Customer>> GetRegisteredBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit);
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}
