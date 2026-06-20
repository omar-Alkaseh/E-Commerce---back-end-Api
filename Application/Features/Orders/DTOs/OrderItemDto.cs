namespace Application.Features.Orders.DTOs
{
    public class OrderItemDto
    {
        public int OrderItemId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal SubTotal => Quantity * UnitPrice;
    }
}
