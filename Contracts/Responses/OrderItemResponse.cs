namespace Contracts.Responses
{
    public class OrderItemResponse
    {
        public int OrderItemId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
