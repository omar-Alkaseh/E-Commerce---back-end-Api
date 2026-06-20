using Application.Common.Constants;
using Application.Common.Models;
using Application.Features.Products.DTOs;
using Application.Features.Reviews.DTOs;
using Application.Interfaces.Services;
using Contracts.Requests;
using Contracts.Responses;
using ECommerce.Api.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Route("api/products")]
    [Authorize]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;

        public ProductController(IProductService productService, IReviewService reviewService)
        {
            _productService = productService;
            _reviewService = reviewService;
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet]
        public async Task<ActionResult<List<ProductResponse>>> GetAll()
        {
            var products = await _productService.GetAllAsync();

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("paged")]
        public async Task<ActionResult<PaginatedResult<ProductResponse>>> GetPaged([FromQuery] int pageNumber = 1,  [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetPagedAsync(pageNumber, pageSize);

            return Ok(new PaginatedResult<ProductResponse>
            {
                Items = result.Items.Select(p => p.ToResponse()).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = result.TotalCount
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponse>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            return Ok(product!.ToResponse());
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("{id:int}/with-images")]
        public async Task<ActionResult<ProductResponse>> GetByIdWithImages(int id)
        {
            var product = await _productService.GetByIdWithImagesAsync(id);

            return Ok(product!.ToResponse());
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<List<ProductResponse>>> GetByCategoryId(int categoryId)
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("min-rating/{minRating:int}")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByMinimumRating(int minRating)
        {
            var products = await _productService.GetProductsByMinimumRatingAsync(minRating);

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }


        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("top-rated")]
        public async Task<ActionResult<List<ProductResponse>>> GetTopRatedProducts()
        {
            var products = await _productService.GetTopRatedProductsAsync();

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }




        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("featured")]
        public async Task<ActionResult<List<ProductResponse>>> GetFeatured()
        {
            var products = await _productService.GetFeaturedAsync();

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }



        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("active")]
        public async Task<ActionResult<List<ProductResponse>>> GetActive()
        {
            var products = await _productService.GetActiveProductsAsync();

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }




        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpGet("search")]
        public async Task<ActionResult<List<ProductResponse>>> Search([FromQuery] string term)
        {
            var products = await _productService.SearchAsync(term);

            return Ok(products.Select(p => p.ToResponse()).ToList());
        }





        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateAsync(new CreateProductDto
            {
                ProductName = request.ProductName,
                Description = request.Description,
                Sku = request.Sku,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                CategoryId = request.CategoryId,
                StockQuantity = request.StockQuantity,
                IsFeatured = request.IsFeatured,
            });

            return CreatedAtAction(nameof(GetById), new { id = product.ProductId}, product.ToResponse());
        }





        [HttpPost("{productId:int}/reviews")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ReviewResponse>> CreateReview([FromRoute] int productId, [FromBody] CreateReviewRequest request)
        {
            var review = await _reviewService.CreateReviewAsync(productId, new CreateReviewDto
            {
                Rating = request.Rating,
                Comment = request.Comment,
            });

            return CreatedAtAction(
                nameof(ReviewController.GetByReviewId),
                "Review",
                new { reviewId = review.ReviewId },
                review.ToResponse());
        }





        [HttpPatch("{reviewId:int}/reviews")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ReviewResponse>> PatchReview([FromRoute] int reviewId, [FromBody] PatchReviewRequest request)
        {
            var review = await _reviewService.PatchReviewAsync(reviewId, new PatchReviewDto
            {
                Rating = request.Rating,
                Comment = request.Comment,
            });

            return Ok(review.ToResponse());
        }

        



        [HttpGet("{productId:int}/reviews/approved")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetByProductId(int productId)
        {
            var reviews = await _reviewService.GetAllByProductIdAsync(productId);

            return Ok(reviews.ToResponseList());
        }





        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpdateProductRequest request)
        {
            var product = await _productService.UpdateAsync(new UpdateProductDto
            {
                ProductId = id,
                ProductName = request.ProductName,
                Description = request.Description,
                Sku = string.IsNullOrWhiteSpace(request.Sku) ? string.Empty : request.Sku,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                CategoryId = request.CategoryId,
                StockQuantity = request.StockQuantity,
                IsFeatured = request.IsFeatured,
                IsActive = request.IsActive,
            });

            return Ok(product.ToResponse());
        }




        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }





        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [HttpPatch("{id:int}")]
        public async Task<ActionResult<ProductResponse>> Patch(int id, [FromBody] PatchProductRequest request)
        {
            var product = await _productService.PatchAsync(new PatchProductDto
            {
                ProductId = id,
                ProductName = request.ProductName,
                Description = request.Description,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                CategoryId = request.CategoryId,
                Sku = request.Sku,
                StockQuantity = request.StockQuantity,
                ClearDiscountPrice = request.ClearDiscountPrice,
                IsFeatured = request.IsFeatured,
                IsActive = request.IsActive,
            });

            return Ok(product.ToResponse());
        }
    }
}
