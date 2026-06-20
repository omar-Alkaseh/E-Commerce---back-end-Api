namespace Application.Features.Products.DTOs
{
    public class UpdateProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public string Sku { get; set; } = null!;

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }

    }
}
