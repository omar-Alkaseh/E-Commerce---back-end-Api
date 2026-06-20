namespace Contracts.Requests
{
    public class PlaceOrderRequest
    {
        public string ShippingAddressLine { get; set; } = null!;

        public string ShippingCity { get; set; } = null!;

        public string ShippingCountry { get; set; } = null!;

        public string? ShippingPostalCode { get; set; }
    }
}
