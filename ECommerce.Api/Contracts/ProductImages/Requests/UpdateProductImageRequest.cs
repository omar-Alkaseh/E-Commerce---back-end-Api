namespace ECommerce.Contracts.ProductImages.Requests
{
    public class UpdateProductImageRequest
    {
        public IFormFile? File { get; set; }

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int SortOrder { get; set; }
    }
}
