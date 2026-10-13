using AiBusinessAssistant.DTOs.Customer;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Repositories.Interfaces;
using AiBusinessAssistant.Services.Interfaces;

namespace AiBusinessAssistant.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var c = await _customerRepository.GetByIdAsync(id);
            if (c == null) return null;

            return new CustomerDto
            {
                CustomerId = c.CustomerId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            };
        }

        public async Task<CustomerDto> CreateAsync(UpsertCustomerDto dto)
        {
            var entity = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address
            };

            var created = await _customerRepository.AddAsync(entity);

            return new CustomerDto
            {
                CustomerId = created.CustomerId,
                Name = created.Name,
                Email = created.Email,
                Phone = created.Phone,
                Address = created.Address,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task UpdateAsync(int id, UpsertCustomerDto dto)
        {
            var existing = await _customerRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("Customer not found");

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.Address = dto.Address;

            await _customerRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id) => await _customerRepository.DeleteAsync(id);
    }
}
