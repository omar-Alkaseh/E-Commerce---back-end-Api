using Application.Common.Constants;
using Application.Interfaces.Services;
using Contracts.Responses;
using ECommerce.Api.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Authorize]
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }


        [AllowAnonymous]
        [HttpGet("{reviewId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ReviewResponse>> GetByReviewId(int reviewId)
        {
            var review = await _reviewService.GetByReviewIdWithProductDetailsAsync(reviewId);

            return Ok(review.ToResponse());
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet("pending")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetPendingReviewsAsync()
        {
            var reviews = await _reviewService.GetPendingReviewsAsync();

            return Ok(reviews.ToResponseList());
        }




        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPut("{reviewId:int}/approve")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ReviewResponse>> AdminApproveReview(int reviewId)
        {
            var review = await _reviewService.AdminApproveReviewAsync(reviewId);

            return Ok(review.ToResponse());
        }


    }
}
