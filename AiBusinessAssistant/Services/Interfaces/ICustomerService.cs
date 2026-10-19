using AiBusinessAssistant.DTOs.Customer;

namespace AiBusinessAssistant.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<IEnumerable<CustomerDto>> GetTopAsync(int limit);
        Task<IEnumerable<CustomerDto>> SearchAsync(string query, int limit);
        Task<IEnumerable<CustomerDto>> GetRegisteredBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit);
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(UpsertCustomerDto dto);
        Task UpdateAsync(int id, UpsertCustomerDto dto);
        Task DeleteAsync(int id);
    }
}
