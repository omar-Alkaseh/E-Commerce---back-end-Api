using Application.Interfaces.Services;
using Contracts.Responses;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<DashboardSummaryResponse> GetSummaryAsync()
        {
            var paidStatus = (byte)PaymentStatus.EnPaymentStatus.Paid;
            var pendingOrderStatus = (byte)OrderStatus.EnOrderStatus.Pending;
            var pendingShipmentStatus = (byte)ShippingStatus.EnShippingStatus.Pending;

            return new DashboardSummaryResponse
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalProducts = await _context.Products.CountAsync(),

                TotalOrders = await _context.Orders.CountAsync(),

                TotalRevenue = await _context.Payments
                    .Where(p => p.PaymentStatus == paidStatus)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0,

                PendingOrders = await _context.Orders
                    .CountAsync(o => o.OrderStatus == pendingOrderStatus),

                PendingReviews = await _context.Reviews
                    .CountAsync(r => !r.IsApproved),

                PendingShipments = await _context.Shipments
                    .CountAsync(s => s.ShippingStatus == pendingShipmentStatus),

                LowStockProducts = await _context.Products
                    .CountAsync(p => p.StockQuantity <= 5)
            };
        }


        public async Task<List<TopProductResponse>> GetTopProductsAsync(int count = 5)
        {
            return await _context.OrderItems
                .AsNoTracking()
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    oi.ProductName
                })
                .Select(g => new TopProductResponse
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .ToListAsync();
        }
    }
}
