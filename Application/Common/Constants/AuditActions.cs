namespace Application.Common.Constants
{
    public static class AuditActions
    {
        public const string LoginFailed = "Login failed";
        public const string OrderPlaced = "Order placed";
        public const string PaymentCreated = "Payment created";
        public const string PaymentProcessed = "Payment processed";
        public const string PaymentRefunded = "Payment refunded";
        public const string CodConfirmed = "COD confirmed";
        public const string ShipmentCreated = "Shipment created";
        public const string ShipmentStatusChanged = "Shipment status changed";
        public const string ReviewCreated = "Review created";
        public const string ReviewApproved = "Review approved";
        public const string ProductCreated = "Product created";
        public const string ProductUpdated = "Product updated";
        public const string UserCreated = "User created";
        public const string AdminCreated = "Admin created";
        public const string UserDeleted = "User deleted";
    }

    public static class AuditEntities
    {
        public const string User = "User";
        public const string Order = "Order";
        public const string Payment = "Payment";
        public const string Shipment = "Shipment";
        public const string Review = "Review";
        public const string Product = "Product";
    }
}
