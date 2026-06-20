namespace Application.Features.ProductImages.DTOs
{
    public class UpdateProductImageDto
    {
        public int ProductId { get; set; }
        public int ProductImageId { get; set; }

        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }

        public string? AltText { get; set; }
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }
}