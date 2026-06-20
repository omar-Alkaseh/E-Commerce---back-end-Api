namespace Application.Features.Reviews.DTOs
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ReviewProductDto? product { get; set; }
    }
}
