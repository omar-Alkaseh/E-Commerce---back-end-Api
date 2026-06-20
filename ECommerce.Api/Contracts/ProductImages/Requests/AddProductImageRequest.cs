namespace ECommerce.Contracts.ProductImages.Requests
{
    public class AddProductImageRequest
    {
        public IFormFile File { get; set; } = null!;

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int? SortOrder { get; set; }
    }
}
