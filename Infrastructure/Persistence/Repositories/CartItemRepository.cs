using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CartItemRepository : BaseRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<CartItem?> GetByCartAndProductAsync(int CartId, int productId) =>
            await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartId == CartId && c.ProductId == productId);

        public async Task<CartItem?> GetByIdWithCartAndProductAsync(int cartItemId) =>
            await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
    }
}
