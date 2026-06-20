namespace Contracts.Requests
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
