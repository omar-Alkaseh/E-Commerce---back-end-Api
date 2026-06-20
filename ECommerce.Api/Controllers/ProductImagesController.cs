using Application.Common.Constants;
using Application.Features.ProductImages.DTOs;
using Application.Interfaces.Services;
using Contracts.Responses;
using ECommerce.Api.Mappings;
using ECommerce.Contracts.ProductImages.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Route("api/products/{productId:int}/images")]
    [ApiController]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImagesController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [AllowAnonymous]
        [EnableRateLimiting("UserLimiter")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<List<ProductImageResponse>>> GetByProductId(int productId)
        {
            var images = await _productImageService.GetByProductIdAsync(productId);

            return Ok(images.Select(pi => pi.ToResponse()));
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ProductImageResponse>> Add(int productId, [FromForm] AddProductImageRequest request)
        {
            if (request.File is null || request.File.Length == 0)
                throw new InvalidOperationException("Image file is required.");

            await using var stream = request.File.OpenReadStream();

            var image = await _productImageService.AddAsync(new AddProductImageDto
            {
                ProductId = productId,
                FileStream = stream,
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                AltText = request.AltText,
                IsMain = request.IsMain
            });

            return CreatedAtAction(
                nameof(GetByProductId),
                new { productId },
                image.ToResponse());
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [HttpPut("{imageId:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ProductImageResponse>> Update(int productId, int imageId,  [FromForm] UpdateProductImageRequest request)
        {
            Stream? stream = null;

            try
            {
                if (request.File is not null && request.File.Length > 0)
                    stream = request.File.OpenReadStream();

                var image = await _productImageService.UpdateAsync(new UpdateProductImageDto
                {
                    ProductId = productId,
                    ProductImageId = imageId,
                    FileStream = stream,
                    FileName = request.File?.FileName,
                    ContentType = request.File?.ContentType,
                    AltText = request.AltText,
                    IsMain = request.IsMain
                });

                return Ok(image.ToResponse());
            }
            finally
            {
                stream?.Dispose();
            }
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [HttpDelete("{imageId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Delete(int productId, int imageId)
        {
            await _productImageService.DeleteAsync(productId, imageId);

            return NoContent();
        }



        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [HttpPut("{imageId:int}/set-main")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> SetMain(int productId, int imageId)
        {
            await _productImageService.SetMainAsync(productId, imageId);

            return NoContent();
        }
    }
}
