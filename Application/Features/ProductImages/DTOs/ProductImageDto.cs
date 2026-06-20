namespace Application.Features.ProductImages.DTOs
{
    public class ProductImageDto
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}