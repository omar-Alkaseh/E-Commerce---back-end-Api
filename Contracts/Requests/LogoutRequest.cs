namespace Contracts.Requests
{
    public class LogoutRequest
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
