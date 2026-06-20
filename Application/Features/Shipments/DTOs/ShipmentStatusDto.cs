using Domain.Enums;

namespace Application.Features.Shipments.DTOs
{
    public class ShipmentStatusDto
    {
        public ShippingStatus.EnShippingStatus Status { get; set; }
    }
}
