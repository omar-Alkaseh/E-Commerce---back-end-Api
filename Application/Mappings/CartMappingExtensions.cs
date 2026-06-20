using Application.Features.Carts.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class CartMappingExtensions
    {
        public static CartDto ToDto(this Cart cart)
        {
            return new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                Items = cart.CartItems.Select(ci => ci.ToDto()).ToList(),
                TotalAmount = cart.CartItems.Sum(ci => GetProductPrice(ci.Product) * ci.Quantity)
            };
        }

        public static CartItemDto ToDto(this CartItem cartItem)
        {
            var unitPrice = GetProductPrice(cartItem.Product);

            return new CartItemDto
            {
                CartItemId = cartItem.CartItemId,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                ImageUrl = cartItem.Product.ProductImages
                    .FirstOrDefault(img => img.IsMain)?.ImageUrl,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                SubTotal = unitPrice * cartItem.Quantity
            };
        }

        private static decimal GetProductPrice(Product product)
        {
            return product.DiscountPrice.HasValue &&  product.DiscountPrice > 0 ? product.DiscountPrice.Value : product.Price;
        }
    }
}
