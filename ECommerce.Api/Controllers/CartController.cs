using Application.Features.Carts.DTOs;
using Application.Interfaces.Services;
using Contracts.Requests;
using ECommerce.Api.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Authorize]
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetMyCart()
        {
            var cart = await _cartService.GetMyCartAsync();

            return Ok(cart.ToResponse());
        }

        [HttpPost("items")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var item = await _cartService.AddToCartAsync(new AddToCartDto
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
            });

            return Ok(item);
        }

        [HttpPut("items/{cartItemId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemQuantityRequest request)
        {
            var updatedItem = await _cartService.UpdateCartItemQuantityAsync(cartItemId, new UpdateCartItemQuantityDto
            {
                Quantity = request.Quantity,
            });

            return Ok(updatedItem.ToResponse());
        }

        [HttpDelete("items/{cartItemId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var cart = await _cartService.RemoveCartItemAsync(cartItemId);

            return Ok(cart.ToResponse());
        }

        [HttpDelete]
        [EnableRateLimiting("UserLimiter")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearAsync();

            return NoContent();
        }
    }
}
