namespace Application.Features.Shipments.DTOs
{
    public class ShipmentOrderDto
    {
        public string OrderNumber { get; set; } = null!;
        public string OrderStatus { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        public string ShippingAddressLine { get; set; } = null!;
        public string ShippingCity { get; set; } = null!;
        public string ShippingCountry { get; set; } = null!;
        public string? ShippingPostalCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
