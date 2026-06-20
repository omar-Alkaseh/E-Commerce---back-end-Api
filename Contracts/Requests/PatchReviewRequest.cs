namespace Contracts.Requests
{
    public class PatchReviewRequest
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }
}
