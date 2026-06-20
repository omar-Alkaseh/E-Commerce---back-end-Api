using Application.Common.Constants;
using Application.Interfaces.Services;
using Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet("summary")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<DashboardSummaryResponse>> GetSummary()
        {
            var summary = await _dashboardService.GetSummaryAsync();

            return Ok(summary);
        }



        [HttpGet("top-products")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(List<TopProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<List<TopProductResponse>>> GetTopProducts([FromQuery] int count = 5)
        {
            var products = await _dashboardService.GetTopProductsAsync(count);

            return Ok(products);
        }
    }
}
