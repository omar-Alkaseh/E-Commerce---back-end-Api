namespace Contracts.Responses
{
    public class CartResponse
    {
        public int CartId { get; set; }

        public int CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        public List<CartItemResponse> Items { get; set; } = new();
    }
}
