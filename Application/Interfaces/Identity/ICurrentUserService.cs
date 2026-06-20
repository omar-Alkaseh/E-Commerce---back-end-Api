namespace Application.Interfaces.Identity
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Email { get; }
        IReadOnlyList<string> Roles { get; }
        bool IsAuthenticated { get; }
        string? IpAddress { get; }
    }
}
