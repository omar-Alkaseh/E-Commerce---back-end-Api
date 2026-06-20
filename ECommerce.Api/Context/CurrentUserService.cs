using Application.Interfaces.Identity;
using System.Security.Claims;
namespace ECommerce.Api.Context
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                return int.TryParse(value, out var userId)
                    ? userId
                    : null;
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.Email)?
                .Value;

        public string? Role =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.Role)?
                .Value;

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?
                .User?
                .Identity?
                .IsAuthenticated ?? false;

        public string? IpAddress 
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;

                if (context == null)
                    return null;

                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(forwardedFor))
                    return forwardedFor.Split(',')[0].Trim();

                return context.Connection.RemoteIpAddress?.ToString();
            }
        }

        public IReadOnlyList<string> Roles => 
            _httpContextAccessor.HttpContext?
            .User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
    }

}

