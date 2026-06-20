using Application.Common.Constants;
using Application.Features.Orders.DTOs;
using Application.Interfaces.Services;
using Contracts.Requests;
using Contracts.Responses;
using ECommerce.Api.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("my-orders")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<List<OrderResponse>>> GetMyOrders()
        {
            var orders = await _orderService.GetMyOrdersAsync();

            return Ok(orders.ToResponseList());
        }

        [HttpGet("{orderId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<OrderResponse>> GetById(int orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);

            return Ok(order!.ToResponse());
        }



        [HttpPost("place-order")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<OrderResponse>> PlaceOrder([FromBody] PlaceOrderRequest request)
        {

            var order = await _orderService.PlaceOrderAsync(new PlaceOrderDto
            {
                ShippingAddressLine = request.ShippingAddressLine,
                ShippingCity = request.ShippingCity,
                ShippingCountry = request.ShippingCountry,
                ShippingPostalCode = request.ShippingPostalCode
            });

            return CreatedAtAction(
                nameof(GetById),
                new { orderId = order.OrderId },
                order.ToResponse());
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<List<OrderResponse>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();

            return Ok(orders.ToResponseList());
        }


        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet("status")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<List<OrderResponse>>> GetOrdersByStatus([FromQuery] GetOrdersRequest request)
        {
            var status = new OrderStatusDto
            {
                Status = request.Status,
            };

            var orders = await _orderService.GetOrdersByStatusAsync(status);

            return Ok(orders.ToResponseList());
        }


        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet("my-orders/status")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<List<OrderResponse>>> GetMyOrdersByStatus([FromQuery] GetOrdersRequest request)
        {
            var status = new OrderStatusDto
            {
                Status = request.Status,
            };

            var orders = await _orderService.GetMyOrdersByStatusAsync(status);

            return Ok(orders.ToResponseList());
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPut("{orderId:int}/status")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<OrderResponse>> UpdateStatus(
            int orderId,
            [FromQuery] GetOrdersRequest request)
        {
            var status = new OrderStatusDto
            {
                Status = request.Status,
            };

            var order = await _orderService.UpdateStatusAsync(orderId, status);

            return Ok(order.ToResponse());
        }
    }
}
