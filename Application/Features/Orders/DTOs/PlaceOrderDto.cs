namespace Application.Features.Orders.DTOs
{
    public class PlaceOrderDto
    {

        public string ShippingAddressLine { get; set; } = null!;

        public string ShippingCity { get; set; } = null!;

        public string ShippingCountry { get; set; } = null!;

        public string? ShippingPostalCode { get; set; }

    }
}
