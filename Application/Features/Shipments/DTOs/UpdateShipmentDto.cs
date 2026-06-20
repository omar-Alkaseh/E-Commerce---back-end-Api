using Domain.Enums;

namespace Application.Features.Shipments.DTOs
{
    public class UpdateShipmentDto
    {
        public string? TrackingNumber { get; set; }
        public string? CarrierName { get; set; }
    }
}
