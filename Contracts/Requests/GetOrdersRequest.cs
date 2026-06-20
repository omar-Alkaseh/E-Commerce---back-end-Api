using Domain.Enums;

namespace Contracts.Requests
{
    public class GetOrdersRequest
    {
        public OrderStatus.EnOrderStatus Status { get; set; }
    }
}
