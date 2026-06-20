using Domain.Enums;

namespace Contracts.Requests
{
    public class UpdateShipmentStatusRequest
    {
        public string? TrackingNumber { get; set; }
        public string? CarrierName { get; set; }
    }
}
