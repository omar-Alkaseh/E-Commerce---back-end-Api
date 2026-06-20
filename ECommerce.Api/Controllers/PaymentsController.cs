using Application.Common.Constants;
using Application.Features.Payments.DTOs;
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
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        [HttpPost("orders/{orderId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<PaymentResponse>> CreatePayment(int orderId, [FromQuery] CreatePaymentRequest request)
        {
            var payment = await _paymentService.CreatePaymentAsync(orderId, new CreatePaymentDto
            {
                PaymentMethod = request.PaymentMethod
            });

            return CreatedAtAction(
                nameof(GetByOrderId),
                new { orderId = payment.OrderId },
                payment.ToResponse());
        }



        [HttpGet("orders/{orderId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetByOrderId(int orderId)
        {
            var payments = await _paymentService.GetByOrderIdAsync(orderId);

            return Ok(payments.ToResponseList());
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPost("{paymentId:int}/process")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<PaymentResponse>> ProcessFakePayment(int paymentId, [FromQuery] ProcessPaymentRequest request)
        {
            var payment = await _paymentService.ProcessFakePaymentAsync(paymentId, request.IsSuccess, request.FailureReason);

            return Ok(payment.ToResponse());
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPost("{paymentId:int}/refund")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<PaymentResponse>> Refund(int paymentId)
        {
            var payment = await _paymentService.RefundAsync(paymentId);

            return Ok(payment.ToResponse());
        }



        [HttpGet("me")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetMyPayments()
        {
            var payments = await _paymentService.GetMyPaymentsAsync();

            return Ok(payments.ToResponseList());
        }




        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();

            return Ok(payments.ToResponseList());
        }
    }
}
