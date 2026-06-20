using Application.Features.Orders.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings
{
    public static class OrderMappingExtensions
    {
        public static OrderDto ToDto(this Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                OrderStatusName = ((OrderStatus.EnOrderStatus)order.OrderStatus).ToString(),
                TotalAmount = order.TotalAmount,

                ShippingAddressLine = order.ShippingAddressLine,
                ShippingCity = order.ShippingCity,
                ShippingCountry = order.ShippingCountry,
                ShippingPostalCode = order.ShippingPostalCode,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    OrderItemId = item.OrderItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,

                }).ToList()
            };
        }

        public static List<OrderDto> ToDtoList(this IEnumerable<Order> order) =>
            order.Select(o => o.ToDto()).ToList();
    }
}
