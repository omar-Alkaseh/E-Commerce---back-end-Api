namespace Application.Interfaces.Services
{
    public interface ISkuGenerator
    {
        string GenerateSku(string productName, int categoryId);
    }
}
