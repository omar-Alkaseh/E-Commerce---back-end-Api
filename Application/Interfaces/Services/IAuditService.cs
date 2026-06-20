namespace Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string entityName,
            int? entityId = null,
            string? oldValues = null,
            string? newValues = null);
    }
}
