namespace Contracts.Responses
{
    public class ReviewProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Slug { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
