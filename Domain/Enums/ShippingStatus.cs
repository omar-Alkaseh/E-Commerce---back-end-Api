namespace Domain.Enums
{
    public class ShippingStatus
    {
        public enum EnShippingStatus
        {
            Pending = 1,
            Shipped = 2,
            InTransit = 3,
            Delivered = 4,
            Returned = 5
        };
    }
}
