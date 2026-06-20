namespace Contracts.Responses
{
    public class DashboardSummaryResponse
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public int PendingOrders { get; set; }
        public int PendingReviews { get; set; }
        public int PendingShipments { get; set; }
        public int LowStockProducts { get; set; }
    }
}
