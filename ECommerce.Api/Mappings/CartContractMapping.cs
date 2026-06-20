using Application.Features.Carts.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class CartContractMapping
    {
        public static CartResponse ToResponse(this CartDto cart)
        {
            return new CartResponse
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                TotalAmount = cart.TotalAmount,
                Items = cart.Items.Select(i => new CartItemResponse
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.SubTotal
                }).ToList()
            };
        }

        public static IEnumerable<CartResponse> ToResponseList(this IEnumerable<CartDto> carts) =>
            carts.Select(c => c.ToResponse()).ToList();
    }
}
