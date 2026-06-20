using Application.Features.ProductImages.DTOs;

namespace Application.Features.Products.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public string Sku { get; set; } = null!;
        public string? Slug { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }

        public string? MainImageUrl { get; set; }

        public decimal? AverageRating { get; set; }

        public int? ReviewsCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ProductImageDto> Images { get; set; } = new();
    }
}
