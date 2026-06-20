using Application.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(
                configuration.GetSection("jwt"));


            services.AddScoped<IPasswordHasher, PasswordHash>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IAuditService, AuditService>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            

            return services;
        }
    }
}
