using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<Customer?> GetByIdWithCart(int id) =>
             await _context.Customers
                .Include(c => c.Cart)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

        public async Task<Customer?> GetByIdWithOrders(int id) =>
             await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        public async Task<Customer?> GetByIdWithReviews(int id) =>
             await _context.Customers
            .Include(c => c.Reviews)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        public async Task<Customer?> GetByUserIdAsync(int userId) =>
            await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<Customer?> GetByUserIdWithDetailsAsync(int userId) =>
            await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Cart)
                    .ThenInclude(c => c!.CartItems)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.UserId == userId);
    }

}
