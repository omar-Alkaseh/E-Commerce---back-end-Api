using Application.Features.Orders.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync();
        Task<OrderDto> PlaceOrderAsync(PlaceOrderDto request);
        Task<OrderDto?> GetByIdAsync(int orderId);
        Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatusDto status);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatusDto status);
        Task<IEnumerable<OrderDto>> GetMyOrdersByStatusAsync(OrderStatusDto status);
    }
}
