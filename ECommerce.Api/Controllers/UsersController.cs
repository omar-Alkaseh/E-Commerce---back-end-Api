using Application.Common.Constants;
using Application.Features.Users.DTOs;
using Application.Interfaces.Services;
using Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();

            return Ok(users);
        }


        [HttpGet("{userId:int}")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetById(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);

            return Ok(user);
        }



        [HttpPost]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]

        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = await _userService.CreateUserAsync(new CreateUserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password
            });

            return CreatedAtAction(nameof(GetById), new { userId = user.UserId }, user);
        }

        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPost("admin")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateUserRequest request)
        {
            var user = await _userService.CreateAdminAsync(new CreateUserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password
            });

            return CreatedAtAction(nameof(GetById), new { userId = user.UserId }, user);
        }



        [HttpPut("{userId:int}")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Update(int userId, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.UpdateAsync(userId, new UpdateUserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.IsActive
            });

            return Ok(user);
        }


        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{userId:int}/admin")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> MakeAdmin(int userId)
        {
            await _userService.MakeAdminAsync(userId);

            return NoContent();
        }



        [HttpDelete("{userId:int}")]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Delete(int userId)
        {
            await _userService.DeleteAsync(userId);

            return NoContent();
        }
    }
}
