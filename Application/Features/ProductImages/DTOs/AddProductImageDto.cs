namespace Application.Features.ProductImages.DTOs
{
    public class AddProductImageDto
    {
        public int ProductId { get; set; }
        public Stream FileStream {get; set;} = null!;
        public string FileName {get; set;} = null!;
        public string ContentType { get; set; } = null!;
        public string? AltText { get; set; }
        public int? SortOrder { get; set; }
        public bool IsMain { get; set; }
    }
}