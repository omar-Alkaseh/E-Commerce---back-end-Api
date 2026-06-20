namespace Application.Interfaces.Identity
{
    public interface IRefreshTokenGenerator
    {
        string Generate();
        string HashToken(string token);
    }
}
