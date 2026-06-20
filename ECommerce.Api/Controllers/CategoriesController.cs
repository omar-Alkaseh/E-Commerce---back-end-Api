using Application.Common.Constants;
using Application.Features.Categories.DTOs;
using Application.Interfaces.Services;
using Contracts.Requests;
using ECommerce.Api.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]

        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();

            return Ok(categories.ToResponseList());
        }

        [AllowAnonymous]
        [HttpGet("tree")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetTree()
        {
            var categories = await _categoryService.GetTreeAsync();

            return Ok(categories.ToResponseList());
        }

        [AllowAnonymous]
        [HttpGet("{categoryId:int}")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetById(int categoryId)
        {
            var category = await _categoryService.GetByIdAsync(categoryId);

            return Ok(category.ToResponse());
        }

        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPost]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var category = await _categoryService.CreateAsync(new CreateCategoryDto
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                IsActive = request.IsActive
            });

            return CreatedAtAction(
                nameof(GetById),
                new { categoryId = category.CategoryId },
                category.ToResponse());
        }

        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPut("{categoryId:int}")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Update(
            int categoryId,
            [FromBody] UpdateCategoryRequest request)
        {
            var category = await _categoryService.UpdateAsync(categoryId, new UpdateCategoryDto
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                IsActive = request.IsActive
            });

            return Ok(category.ToResponse());
        }

        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpDelete("{categoryId:int}")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Delete(int categoryId)
        {
            await _categoryService.DeleteAsync(categoryId);

            return NoContent();
        }
    }
}
