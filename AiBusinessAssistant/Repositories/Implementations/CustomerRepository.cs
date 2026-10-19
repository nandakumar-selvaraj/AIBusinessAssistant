using AiBusinessAssistant.Data;
using AiBusinessAssistant.Entities;
using AiBusinessAssistant.Common.Exceptions;
using AiBusinessAssistant.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BusinessAiDbContext _context;

        public CustomerRepository(BusinessAiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync() =>
            await _context.Customers.AsNoTracking().OrderBy(customer => customer.Name).ToListAsync();

        public async Task<IEnumerable<Customer>> GetTopAsync(int limit) =>
            await _context.Customers.AsNoTracking().OrderBy(c => c.Name).Take(limit).ToListAsync();

        public async Task<IEnumerable<Customer>> SearchAsync(string query, int limit) =>
            await _context.Customers.AsNoTracking()
                .Where(c => EF.Functions.Like(c.Name, $"%{EscapeLike(query)}%", "\\") ||
                    (c.Email != null && EF.Functions.Like(c.Email, $"%{EscapeLike(query)}%", "\\")))
                .OrderBy(c => c.Name).Take(limit).ToListAsync();

        public async Task<IEnumerable<Customer>> GetRegisteredBetweenAsync(DateTime startInclusive, DateTime endExclusive, int limit) =>
            await _context.Customers.AsNoTracking()
                .Where(customer => customer.CreatedAt >= startInclusive && customer.CreatedAt < endExclusive)
                .OrderByDescending(customer => customer.CreatedAt).ThenBy(customer => customer.CustomerId)
                .Take(limit).ToListAsync();

        private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        public async Task<Customer?> GetByIdAsync(int id) =>
            await _context.Customers.Include(c => c.Orders).FirstOrDefaultAsync(c => c.CustomerId == id);

        public async Task<Customer> AddAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer is null)
                throw new ApiNotFoundException($"Customer {id} was not found.");

            if (await _context.Orders.AnyAsync(order => order.CustomerId == id))
                throw new ApiConflictException("This customer has orders and cannot be deleted. Preserve the order history by keeping the customer record.");

            _context.Customers.Remove(customer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ApiConflictException("This customer is referenced by an order and cannot be deleted.");
            }
        }
    }
}
