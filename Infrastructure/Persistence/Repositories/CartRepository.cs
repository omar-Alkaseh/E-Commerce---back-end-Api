using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<Cart?> GetByCustomerIdWithCartItemsAsync(int customerId) =>
            await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);


        public async Task<Cart?> GetByCustomerIdWithItemsProductsAndImagesAsync(int customerId) =>
            await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }
}
