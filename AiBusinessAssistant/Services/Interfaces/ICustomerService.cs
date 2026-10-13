using AiBusinessAssistant.DTOs.Customer;

namespace AiBusinessAssistant.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(UpsertCustomerDto dto);
        Task UpdateAsync(int id, UpsertCustomerDto dto);
        Task DeleteAsync(int id);
    }
}
