using Application.Features.Carts.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface ICartService
    {
        Task<CartDto> GetMyCartAsync();
        Task<CartDto> AddToCartAsync(AddToCartDto request);
        Task<CartDto> UpdateCartItemQuantityAsync(int cartItemId, UpdateCartItemQuantityDto request);
        Task<CartDto> RemoveCartItemAsync(int cartItemID);
        Task ClearAsync();
    }
}
