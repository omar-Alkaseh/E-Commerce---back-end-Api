namespace Application.Features.Carts.DTOs
{
    public class CartItemDto
    {
        public int CartItemId {  get; set; }
        public int ProductId {  get; set; }
        public string ProductName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}
