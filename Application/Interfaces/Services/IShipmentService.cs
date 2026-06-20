using Application.Features.Shipments.DTOs;

namespace Application.Interfaces.Services
{
    public interface IShipmentService
    {
        Task<ShipmentDto> CreateShipmentAsync(int orderId, CreateShipmentDto request);
        Task<ShipmentDto> UpdateShipmentAsync(int shipmentId, ShipmentStatusDto status, UpdateShipmentDto request);
        Task<ShipmentDto> GetByOrderIdAsync(int orderId);
        Task<ShipmentDto> GetByShipmentIdWithOrderDetailsAsync(int shipmentId);
        Task<IReadOnlyList<ShipmentDto>> GetAllAsync();
        Task<IReadOnlyList<ShipmentDto>> GetMyShipmentsAsync();
        Task<IEnumerable<ShipmentDto>> GetByShipmentStatusWithOrderDetails(ShipmentStatusDto status);
    }
}
