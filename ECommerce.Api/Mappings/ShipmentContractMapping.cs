using Application.Features.Shipments.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class ShipmentContractMapping
    {
        public static ShipmentResponse ToResponse(this ShipmentDto shipment)
        {
            return new ShipmentResponse
            {
                ShipmentId = shipment.ShipmentId,
                OrderId = shipment.OrderId,
                Status = shipment.ShipmentStatusName,
                TrackingNumber = shipment.TrackingNumber,
                CarrierName = shipment.CarrierName,
                ShippedAt = shipment.ShippedAt,
                DeliveredAt = shipment.DeliveredAt,
                CreatedAt = shipment.CreatedAt,
                UpdatedAt = shipment.UpdatedAt,

                Order = shipment.Order is null ? null : new ShipmentOrderResponse
                {
                    OrderNumber = shipment.Order.OrderNumber,
                    OrderStatus = shipment.Order.OrderStatus,
                    TotalAmount = shipment.Order.TotalAmount,
                    ShippingAddressLine = shipment.Order.ShippingAddressLine,
                    ShippingCity = shipment.Order.ShippingCity,
                    ShippingCountry = shipment.Order.ShippingCountry,
                    ShippingPostalCode = shipment.Order.ShippingPostalCode,
                    CreatedAt = shipment.Order.CreatedAt
                }
            };
        }

        public static List<ShipmentResponse> ToResponseList(this IEnumerable<ShipmentDto> shipments) =>
            shipments.Select(s => s.ToResponse()).ToList();

    }
}
