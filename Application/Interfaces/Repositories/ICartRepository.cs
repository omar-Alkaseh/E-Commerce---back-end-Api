using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetByCustomerIdWithCartItemsAsync(int customerId);
        Task<Cart?> GetByCustomerIdWithItemsProductsAndImagesAsync(int customerId);
    }
}
