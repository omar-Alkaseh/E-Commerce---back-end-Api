using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Services.Interfaces;
using Contracts.Requests;
using Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private static AuthResponse ToAuthResponse(AuthResult result)
        {
            return new AuthResponse
            {
                AccessToken = result.AccessToken,
                ExpiresAtUtc = result.ExpiresAtUtc,
                RefreshToken = result.RefreshToken,
            };
        }

        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("register")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(new RegisterDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
            });

            return Ok(ToAuthResponse(result));
        }

        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(new LoginDto
            {
                Email = request.Email,
                Password = request.Password
            });

            return Ok(ToAuthResponse(result));
        }

        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("refresh-token")]
        [EnableRateLimiting("UserLimiter")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                RefreshToken = request.RefreshToken
            });

            return Ok(ToAuthResponse(result));
        }

        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("logout")]
        [EnableRateLimiting("UserLimiter")]
        public async Task<ActionResult<LogoutResponse>> Logout([FromBody] LogoutRequest request)
        {
            var dto = new LogoutDto
            {
                RefreshToken = request.RefreshToken
            };

            await _authService.LogoutAsync(dto);

            return Ok(new LogoutResponse
            {
                Message = "Logged out successfully."
            });
        }
    }
}
