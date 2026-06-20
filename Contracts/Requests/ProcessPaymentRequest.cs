namespace Contracts.Requests
{
    public class ProcessPaymentRequest
    {
        public bool IsSuccess { get; set; }
        public string? FailureReason { get; set; }
    }
}
