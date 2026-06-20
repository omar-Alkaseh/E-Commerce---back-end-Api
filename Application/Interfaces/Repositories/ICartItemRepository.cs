using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<CartItem?> GetByCartAndProductAsync(int CartId, int productId);
        Task<CartItem?> GetByIdWithCartAndProductAsync(int cartId);
    }
}
