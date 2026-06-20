namespace ECommerce.Api.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TraceId { get; set; }
    }
}
