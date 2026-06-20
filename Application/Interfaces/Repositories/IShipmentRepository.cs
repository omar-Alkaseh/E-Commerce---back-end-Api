using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IShipmentRepository : IRepository<Shipment>
    {
        Task<IEnumerable<Shipment>> GetMyShipmentAsync(int customerId);
        Task<IEnumerable<Shipment>> GetByShipmentsStatusWithOrderByDetails(ShippingStatus.EnShippingStatus status);
        Task<IReadOnlyList<Shipment>> GetAllWithOrderAsync();
        Task<Shipment?> GetByShipmentIdWithOrderAndPaymentDetails(int shipmentId);
        Task<Shipment?> GetByOrderIdAsync(int orderId);
        Task<bool> ExistsByOrderIdAsync(int orderId);
        
    }
}
