using Application.Features.Shipments.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings
{
    public static class ShipmentMappingExtensions
    {
        public static ShipmentDto ToDto(this Shipment shipment)
        {
            return new ShipmentDto
            {
                ShipmentId = shipment.ShipmentId,
                OrderId = shipment.OrderId,
                ShipmentStatus = shipment.ShippingStatus,
                ShipmentStatusName = ((ShippingStatus.EnShippingStatus)shipment.ShippingStatus).ToString(),
                TrackingNumber = shipment.TrackingNumber,
                CarrierName = shipment.CarrierName,
                ShippedAt = shipment.ShippedAt,
                DeliveredAt = shipment.DeliveredAt,
                CreatedAt = shipment.CreatedAt,
                UpdatedAt = shipment.UpdatedAt,

                Order = shipment.Order is null ? null : new ShipmentOrderDto
                {
                    OrderNumber = shipment.Order.OrderNumber,
                    OrderStatus = ((OrderStatus.EnOrderStatus)shipment.Order.OrderStatus).ToString(),
                    TotalAmount = shipment.Order.TotalAmount,
                    ShippingAddressLine = shipment.Order.ShippingAddressLine,
                    ShippingCity = shipment.Order.ShippingCity,
                    ShippingCountry = shipment.Order.ShippingCountry,
                    ShippingPostalCode = shipment.Order.ShippingPostalCode,
                    CreatedAt = shipment.Order.CreatedAt
                }
            };
        }

        public static List<ShipmentDto> ToDtoList(this IEnumerable<Shipment> shipment) =>
            shipment.Select(s => s.ToDto()).ToList();
    }
}
