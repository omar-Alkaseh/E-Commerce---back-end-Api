namespace Contracts.Responses
{
    public class CategoryResponse
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<CategoryResponse> Children { get; set; } = new();
    }
}
