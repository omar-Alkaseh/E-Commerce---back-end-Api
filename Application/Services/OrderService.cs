using Application.Common.Constants;
using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Features.Orders.DTOs;
using Application.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IValidator<PlaceOrderDto> _placeOrderValidator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICartRepository _cartRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IValidator<OrderStatusDto> _orderStatusValidator;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<OrderService> _logger;
        private readonly IAuditService _auditService;


        public OrderService(IOrderRepository orderRepository, IValidator<PlaceOrderDto> placeOrderValidator,
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ICartRepository cartRepository,
            ICartItemRepository cartItemRepository, IValidator<OrderStatusDto> orderStatusValidator, IShipmentRepository shipmentRepository, 
            ICustomerRepository customerRepository, ILogger<OrderService> logger, IAuditService auditService)
        {
            _orderRepository = orderRepository;
            _placeOrderValidator = placeOrderValidator;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _orderStatusValidator = orderStatusValidator;
            _shipmentRepository = shipmentRepository;
            _customerRepository = customerRepository;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllWithDetailsAsync();

            return orders.ToDtoList();
        }

        public async Task<OrderDto?> GetByIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("Cannot get order with invalid id {OrderId}", orderId);
                throw new BadRequestException("OrderId must be greater than 0.");
            }

            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} was not found", orderId);
                throw new NotFoundException("Order is not found.");
            }

            var customerId = await GetCurrentCustomerId();

            bool isAdmin = _currentUserService.Roles.Contains(Roles.Admin);

            if (order.CustomerId != customerId && !isAdmin)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to access order {OrderId} owned by another customer",
                    customerId,
                    orderId);
                throw new ForbiddenException("You are not allowed to access this order.");
            }

            return order?.ToDto();
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync()
        {
            var customerId = await GetCurrentCustomerId();

            var myOrders = await _orderRepository.GetByCustomerIdAsync(customerId);

            return myOrders.ToDtoList();
        }

        public async Task<OrderDto> PlaceOrderAsync(PlaceOrderDto request)
        {
            var validationResult = await _placeOrderValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var customerId = await GetCurrentCustomerId();

                var cart = await _cartRepository.GetByCustomerIdWithCartItemsAsync(customerId);

                if (cart is null)
                {
                    _logger.LogWarning("Cannot place order because cart was not found for customer {CustomerId}", customerId);
                    throw new NotFoundException("Your cart is not found.");
                }

                if (cart.CartItems is null || !cart.CartItems.Any())
                {
                    _logger.LogWarning("Cannot place order because cart {CartId} is empty", cart.CartId);
                    throw new BadRequestException("Your cart is empty.");
                }

                foreach (var item in cart.CartItems)
                {
                    if (item.Product is null)
                    {
                        _logger.LogWarning(
                            "Cannot place order because product {ProductId} in cart {CartId} was not found",
                            item.ProductId,
                            cart.CartId);
                        throw new NotFoundException("Product not found.");
                    }

                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        _logger.LogWarning(
                            "Insufficient stock for product {ProductId}: requested {RequestedQuantity}, available {StockQuantity}",
                            item.ProductId,
                            item.Quantity,
                            item.Product.StockQuantity);
                        throw new BadRequestException($"Not enough stock for product: {item.Product.Name}");
                    }
                }

                var orderItems = cart.CartItems.Select(cartItem =>
                {
                    var unitPrice = GetUnitPrice(cartItem.Product);

                    return new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * cartItem.Quantity,
                    };
                }).ToList();

                var totalAmount = orderItems.Sum(oi => oi.TotalPrice);

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderNumber = GenerateOrderNumber(),

                    OrderStatus = (byte)OrderStatus.EnOrderStatus.Pending,
                    TotalAmount = totalAmount,

                    ShippingAddressLine = request.ShippingAddressLine,
                    ShippingCity = request.ShippingCity,
                    ShippingCountry = request.ShippingCountry,
                    ShippingPostalCode = request.ShippingPostalCode,

                    CreatedAt = DateTime.UtcNow,

                    OrderItems = orderItems,
                };

                foreach (var cartItem in cart.CartItems)
                    cartItem.Product.StockQuantity -= cartItem.Quantity;


                await _orderRepository.AddAsync(order);

                _cartItemRepository.RemoveRange(cart.CartItems);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                await _auditService.LogAsync(
                    AuditActions.OrderPlaced,
                    AuditEntities.Order,
                    order.OrderId,
                    newValues: JsonSerializer.Serialize(new
                    {
                        Status = OrderStatus.EnOrderStatus.Pending,
                        CustomerId = customerId,
                        ItemCount = orderItems.Count
                    }));

                _logger.LogInformation(
                    "Order {OrderId} placed for customer {CustomerId} with {OrderItemCount} items",
                    order.OrderId,
                    customerId,
                    orderItems.Count);

                return order.ToDto();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency failure while placing an order");
                await _unitOfWork.RollbackTransactionAsync();

                throw new ConflictException("Product stock changed while placing the order. Please refresh your cart and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to place order");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatusDto status)
        {

            var validationResult = await _orderStatusValidator.ValidateAsync(status);
            validationResult.ThrowIfValidationFails();

            if (orderId <= 0)
            {
                _logger.LogWarning("Cannot update status for order with invalid id {OrderId}", orderId);
                throw new BadRequestException("OrderId must be greater than 0.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _orderRepository.GetByOrderIdWithShipment(orderId);

                if (order is null)
                {
                    _logger.LogWarning("Cannot update status because order {OrderId} was not found", orderId);
                    throw new NotFoundException("order is not found.");
                }

                var currentStatus = (OrderStatus.EnOrderStatus)order.OrderStatus;
                var newStatus = status.Status;

                if (newStatus == OrderStatus.EnOrderStatus.Shipped ||
                    newStatus == OrderStatus.EnOrderStatus.Delivered)
                {
                    _logger.LogWarning(
                        "Rejected direct order status change for order {OrderId} from {CurrentStatus} to {NewStatus}",
                        orderId,
                        currentStatus,
                        newStatus);
                    throw new BadRequestException(
                        "Use shipment status update to mark order as shipped or delivered.");
                }

                if (!CanChangeStatus(currentStatus, newStatus))
                {
                    _logger.LogWarning(
                        "Invalid order status transition for order {OrderId} from {CurrentStatus} to {NewStatus}",
                        orderId,
                        currentStatus,
                        newStatus);
                    throw new BadRequestException($"Cannot change order status from {currentStatus} to {newStatus}.");
                }


                var shipment = order.Shipment;

                if (newStatus == OrderStatus.EnOrderStatus.Shipped)
                {
                    if (shipment is null)
                    {
                        _logger.LogWarning("Cannot mark order {OrderId} as shipped because no shipment exists", orderId);
                        throw new BadRequestException("Cannot mark order as shipped before creating shipment.");
                    }

                    shipment.ShippingStatus = (byte)ShippingStatus.EnShippingStatus.Shipped;
                    shipment.ShippedAt ??= DateTime.UtcNow;
                }

                if (newStatus == OrderStatus.EnOrderStatus.Delivered)
                {
                    if (shipment is null)
                    {
                        _logger.LogWarning("Cannot mark order {OrderId} as delivered because no shipment exists", orderId);
                        throw new BadRequestException("Cannot mark order as delivered because shipment does not exist.");
                    }

                    shipment.ShippingStatus = (byte)ShippingStatus.EnShippingStatus.Delivered;
                    shipment.DeliveredAt ??= DateTime.UtcNow;
                }

                order.OrderStatus = (byte)newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                _orderRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Order {OrderId} status changed from {PreviousStatus} to {NewStatus}",
                    orderId,
                    currentStatus,
                    newStatus);

                return order.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order {OrderId}", orderId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatusDto status)
        {
            var validationResult = await _orderStatusValidator.ValidateAsync(status);
            validationResult.ThrowIfValidationFails();

            var orders = await _orderRepository.GetAllWithDetailsAsync(status.Status);

            return orders.ToDtoList();
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersByStatusAsync(OrderStatusDto status)
        {
            var validationResult = await _orderStatusValidator.ValidateAsync(status);
            validationResult.ThrowIfValidationFails();

            var customerId = await GetCurrentCustomerId();
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId, status.Status);

            return orders.ToDtoList();
        }

        private async Task<int> GetCurrentCustomerId()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to access customer orders");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (_currentUserService.UserId is null)
            {
                _logger.LogWarning("Authenticated user is missing a user id claim while accessing customer orders");
                throw new UnauthorizedAccessException("Customer id was not found in token.");
            }

            var customer = await _customerRepository.GetByUserIdAsync(_currentUserService.UserId.Value);

            if (customer is null)
            {
                _logger.LogWarning("User {UserId} is not associated with a customer", _currentUserService.UserId.Value);
                throw new UnauthorizedAccessException("This user is not a customer.");
            }

            return customer.CustomerId;

        }


        private static decimal GetUnitPrice(Product product) =>
            (product.DiscountPrice.HasValue && product.DiscountPrice > 0) ? product.DiscountPrice.Value : product.Price;

        private static string GenerateOrderNumber() =>
            $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";


        private static bool CanChangeStatus(OrderStatus.EnOrderStatus currentStatus, OrderStatus.EnOrderStatus newStatus)
        {
            if (currentStatus == newStatus)
                return true;

            return currentStatus switch
            {
                OrderStatus.EnOrderStatus.Pending =>
                    newStatus == OrderStatus.EnOrderStatus.Processing ||
                    newStatus == OrderStatus.EnOrderStatus.Cancelled,

                OrderStatus.EnOrderStatus.Processing =>
                    newStatus == OrderStatus.EnOrderStatus.Cancelled ,

                OrderStatus.EnOrderStatus.Shipped => false,

                OrderStatus.EnOrderStatus.Delivered => 
                    newStatus == OrderStatus.EnOrderStatus.Returned,

                OrderStatus.EnOrderStatus.Returned => false,

                OrderStatus.EnOrderStatus.Cancelled => false,

                _ => false
            };
        }
    }
}
