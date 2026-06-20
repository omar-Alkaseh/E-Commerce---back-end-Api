namespace Contracts.Responses
{
    public class OrderResponse
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string OrderNumber { get; set; } = null!;

        public byte OrderStatus { get; set; }

        public string OrderStatusName { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public string ShippingAddressLine { get; set; } = null!;

        public string ShippingCity { get; set; } = null!;

        public string ShippingCountry { get; set; } = null!;

        public string? ShippingPostalCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
