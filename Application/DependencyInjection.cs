using Application.Features.Auth.Services;
using Application.Features.Auth.Services.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection 
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ILogoutService, LogoutService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IRegisterService, RegisterService>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductImageService, ProductImageService>();

            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddScoped<IReviewService, ReviewService>();

            services.AddSingleton<ISlugGenerator, SlugGenerator>();
            services.AddSingleton<ISkuGenerator, SkuGenerator>();


            

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
