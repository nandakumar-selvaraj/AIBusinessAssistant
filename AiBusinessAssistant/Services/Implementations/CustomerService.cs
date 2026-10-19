using AiBusinessAssistant.DTOs.Customer;
using AiBusinessAssistant.Common.Exceptions;
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

        public async Task<IEnumerable<CustomerDto>> GetTopAsync(int limit)
        {
            ValidateLimit(limit);
            return (await _customerRepository.GetTopAsync(limit)).Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerDto>> SearchAsync(string query, int limit)
        {
            ValidateLimit(limit);
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length is < 2 or > 100)
                throw new ApiValidationException("Customer search text must be between 2 and 100 characters.");
            return (await _customerRepository.SearchAsync(query.Trim(), limit)).Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerDto>> GetRegisteredBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit)
        {
            ValidateLimit(limit);
            if (startInclusive >= endExclusive) throw new ApiValidationException("Registration date range is invalid.");
            return (await _customerRepository.GetRegisteredBetweenAsync(startInclusive, endExclusive, limit)).Select(MapToDto);
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

        private static CustomerDto MapToDto(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            CreatedAt = c.CreatedAt
        };

        private static void ValidateLimit(int limit)
        {
            if (limit is < 1 or > 20) throw new ApiValidationException("Result limit must be between 1 and 20.");
        }

        public async Task<CustomerDto> CreateAsync(UpsertCustomerDto dto)
        {
            var entity = new Customer
            {
                Name = dto.Name.Trim(),
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
            if (existing == null) throw new ApiNotFoundException($"Customer {id} was not found.");

            existing.Name = dto.Name.Trim();
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.Address = dto.Address;

            await _customerRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id) => await _customerRepository.DeleteAsync(id);
    }
}
