namespace Application.Features.Reviews.DTOs
{
    public class ReviewProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Slug { get; set; }
        public string? MainImageUrl { get; set; }
    }
}
