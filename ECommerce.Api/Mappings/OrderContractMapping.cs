using Application.Features.Orders.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class OrderContractMapping
    {
        public static OrderResponse ToResponse(this OrderDto order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                OrderStatusName = order.OrderStatusName,
                TotalAmount = order.TotalAmount,

                ShippingAddressLine = order.ShippingAddressLine,
                ShippingCity = order.ShippingCity,
                ShippingCountry = order.ShippingCountry,
                ShippingPostalCode = order.ShippingPostalCode,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                Items = order.OrderItems.Select(item => new OrderItemResponse
                {
                    OrderItemId = item.OrderItemId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.SubTotal
                }).ToList()
            };
        }

        public static List<OrderResponse> ToResponseList(this IEnumerable<OrderDto> orders)
        {
            return orders.Select(o => o.ToResponse()).ToList();
        }
    }
}
