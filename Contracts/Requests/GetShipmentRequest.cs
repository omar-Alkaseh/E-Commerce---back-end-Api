using Domain.Enums;

namespace Contracts.Requests
{
    public class GetShipmentRequest
    {
        public ShippingStatus.EnShippingStatus Status { get; set; }
    }
}
