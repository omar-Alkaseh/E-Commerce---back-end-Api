using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Common.Constants;
using Application.Features.Shipments.DTOs;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateShipmentDto> _createShipmentValidator;
        private readonly IValidator<UpdateShipmentDto> _updateShipmentValidator;
        private readonly IValidator<ShipmentStatusDto> _shipmentStatusValidator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<ShipmentService> _logger;
        private readonly IAuditService _auditService;

        public ShipmentService(IShipmentRepository shipmentRepository, IOrderRepository orderRepository, IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService, IValidator<CreateShipmentDto> createShipmentValidator,
            IValidator<UpdateShipmentDto> updateShipmentValidator, ICustomerRepository customerRepository,
            IValidator<ShipmentStatusDto> shipmentStatusValidator, IPaymentRepository paymentRepository,
            ILogger<ShipmentService> logger, IAuditService auditService)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _createShipmentValidator = createShipmentValidator;
            _updateShipmentValidator = updateShipmentValidator;
            _customerRepository = customerRepository;
            _shipmentStatusValidator = shipmentStatusValidator;
            _paymentRepository = paymentRepository;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<ShipmentDto> CreateShipmentAsync(int orderId, CreateShipmentDto request)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("Cannot create shipment for invalid order id {OrderId}", orderId);
                throw new BadRequestException("order id must be greater than 0.");
            }

            var validationResult = await _createShipmentValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order is null)
            {
                _logger.LogWarning("Cannot create shipment because order {OrderId} was not found", orderId);
                throw new NotFoundException("order is not found.");
            }

            if (order.OrderStatus != (byte)OrderStatus.EnOrderStatus.Processing)
            {
                _logger.LogWarning(
                    "Cannot create shipment for order {OrderId} with status {OrderStatus}",
                    orderId,
                    (OrderStatus.EnOrderStatus)order.OrderStatus);
                throw new BadRequestException("shipment can only be created for processing orders.");
            }

            var shipment = new Shipment
            {
                OrderId = orderId,
                ShippingStatus = (byte)ShippingStatus.EnShippingStatus.Pending,
                TrackingNumber = GenerateTrackingNumber(),
                CarrierName = request.CarrierName,
                CreatedAt = DateTime.UtcNow
            };

            await _shipmentRepository.AddAsync(shipment);

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.ShipmentCreated,
                AuditEntities.Shipment,
                shipment.ShipmentId,
                newValues: JsonSerializer.Serialize(new
                {
                    OrderId = orderId,
                    Status = ShippingStatus.EnShippingStatus.Pending
                }));

            _logger.LogInformation("Shipment {ShipmentId} created for order {OrderId}", shipment.ShipmentId, orderId);

            return shipment.ToDto();
        }

        public async Task<IReadOnlyList<ShipmentDto>> GetAllAsync()
        {
            var shipments = await _shipmentRepository.GetAllWithOrderAsync();

            return shipments.ToDtoList();
        }

        public async Task<ShipmentDto> GetByShipmentIdWithOrderDetailsAsync(int shipmentId)
        {
            if (shipmentId <= 0)
            {
                _logger.LogWarning("Cannot get shipment with invalid id {ShipmentId}", shipmentId);
                throw new BadRequestException("shipment id must be greater than 0.");
            }

            var shipment = await _shipmentRepository.GetByShipmentIdWithOrderAndPaymentDetails(shipmentId);

            if (shipment is null)
            {
                _logger.LogWarning("Shipment {ShipmentId} was not found", shipmentId);
                throw new NotFoundException("Shipment is not found.");
            }

            return shipment.ToDto();
        }

        public async Task<ShipmentDto> GetByOrderIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("Cannot get shipment for invalid order id {OrderId}", orderId);
                throw new BadRequestException("order id must be greater than 0.");
            }

            var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId);

            if (shipment is null)
            {
                _logger.LogWarning("Shipment was not found for order {OrderId}", orderId);
                throw new NotFoundException("Shipment is not found.");
            }

            return shipment.ToDto();
        }


        public async Task<IEnumerable<ShipmentDto>> GetByShipmentStatusWithOrderDetails(ShipmentStatusDto status)
        {
            var validationResult = await _shipmentStatusValidator.ValidateAsync(status);
            validationResult.ThrowIfValidationFails();

            var shipments = await _shipmentRepository.GetByShipmentsStatusWithOrderByDetails(status.Status);

            return shipments.ToDtoList();
        }


        public async Task<IReadOnlyList<ShipmentDto>> GetMyShipmentsAsync()
        {
            var customerId = await GetCurrentCustomerId();

            var myShipments = await _shipmentRepository.GetMyShipmentAsync(customerId);

            return myShipments.ToDtoList();
        }

        public async Task<ShipmentDto> UpdateShipmentAsync(int shipmentId, ShipmentStatusDto status, UpdateShipmentDto request)
        {
            if (shipmentId <= 0)
            {
                _logger.LogWarning("Cannot update shipment with invalid id {ShipmentId}", shipmentId);
                throw new BadRequestException("Shipment id must be greater than 0.");
            }

            var validationResult = await _updateShipmentValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var shipment = await _shipmentRepository.GetByShipmentIdWithOrderAndPaymentDetails(shipmentId);

                if (shipment is null)
                {
                    _logger.LogWarning("Cannot update shipment {ShipmentId} because it was not found", shipmentId);
                    throw new NotFoundException("Shipment is not found.");
                }

                if (shipment.Order is null)
                {
                    _logger.LogWarning("Cannot update shipment {ShipmentId} because its order was not loaded", shipmentId);
                    throw new BadRequestException("Order was not loaded with shipment.");
                }


                var currentStatus =  (ShippingStatus.EnShippingStatus)shipment.ShippingStatus;
                var newStatus = status.Status;

                if (!CanChangeShippingStatus(currentStatus, newStatus))
                {
                    _logger.LogWarning(
                        "Invalid shipment status transition for shipment {ShipmentId} from {CurrentStatus} to {NewStatus}",
                        shipmentId,
                        currentStatus,
                        newStatus);
                    throw new BadRequestException($"Cannot change shipment status from {currentStatus} to {newStatus}.");
                }

                shipment.ShippingStatus = (byte)newStatus;
                int? paidCodPaymentId = null;

                switch(newStatus)
                {
                    case ShippingStatus.EnShippingStatus.Shipped:
                        shipment.ShippedAt ??= DateTime.UtcNow;
                        shipment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Shipped;
                        break;

                    case ShippingStatus.EnShippingStatus.InTransit:
                        shipment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Shipped;
                        break;

                    case ShippingStatus.EnShippingStatus.Delivered:
                        shipment.DeliveredAt ??= DateTime.UtcNow;
                        shipment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Delivered;

                        var pendingCodPayments = shipment.Order.Payments
                                .Where(p =>
                                    p.PaymentMethod == (byte)PaymentMethod.EnPaymentMethod.CashOnDelivery &&
                                    p.PaymentStatus == (byte)PaymentStatus.EnPaymentStatus.Pending)
                                .ToList();

                        if (pendingCodPayments.Count > 1)
                        {
                            _logger.LogWarning(
                                "Order {OrderId} has {PaymentCount} pending cash on delivery payments",
                                shipment.OrderId,
                                pendingCodPayments.Count);
                            throw new ConflictException("Order has more than one pending cash on delivery payment.");
                        }

                        var codPayment = pendingCodPayments.FirstOrDefault();

                        if (codPayment is not null)
                        {
                            codPayment.PaymentStatus = (byte)PaymentStatus.EnPaymentStatus.Paid;
                            codPayment.PaidAt = DateTime.UtcNow;
                            paidCodPaymentId = codPayment.PaymentId;
                        }

                        break;

                    case ShippingStatus.EnShippingStatus.Returned:
                        shipment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Returned;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
                    shipment.TrackingNumber = request.TrackingNumber;

                if (!string.IsNullOrWhiteSpace(request.CarrierName))
                    shipment.CarrierName = request.CarrierName;

                shipment.Order.UpdatedAt = DateTime.UtcNow;
                shipment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditService.LogAsync(
                    AuditActions.ShipmentStatusChanged,
                    AuditEntities.Shipment,
                    shipmentId,
                    oldValues: JsonSerializer.Serialize(new { Status = currentStatus }),
                    newValues: JsonSerializer.Serialize(new
                    {
                        Status = newStatus,
                        shipment.OrderId
                    }));

                _logger.LogInformation(
                    "Shipment {ShipmentId} status changed from {PreviousStatus} to {NewStatus} for order {OrderId}",
                    shipmentId,
                    currentStatus,
                    newStatus,
                    shipment.OrderId);

                if (paidCodPaymentId.HasValue)
                {
                    await _auditService.LogAsync(
                        AuditActions.CodConfirmed,
                        AuditEntities.Payment,
                        paidCodPaymentId.Value,
                        oldValues: JsonSerializer.Serialize(new { Status = PaymentStatus.EnPaymentStatus.Pending }),
                        newValues: JsonSerializer.Serialize(new
                        {
                            Status = PaymentStatus.EnPaymentStatus.Paid,
                            shipment.OrderId,
                            ShipmentId = shipmentId
                        }));

                    _logger.LogInformation(
                        "Cash on delivery payment {PaymentId} processed for order {OrderId}",
                        paidCodPaymentId.Value,
                        shipment.OrderId);
                }

                return shipment.ToDto();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update shipment {ShipmentId}", shipmentId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        private async Task<int> GetCurrentCustomerId()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to access customer shipments");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (_currentUserService.UserId is null)
            {
                _logger.LogWarning("Authenticated user is missing a user id claim while accessing customer shipments");
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


        private static bool CanChangeShippingStatus(
            ShippingStatus.EnShippingStatus currentStatus,
            ShippingStatus.EnShippingStatus newStatus)
        {
            if (currentStatus == newStatus)
                return true;

            return currentStatus switch
            {
                ShippingStatus.EnShippingStatus.Pending =>
                    newStatus == ShippingStatus.EnShippingStatus.Shipped,

                ShippingStatus.EnShippingStatus.Shipped =>
                    newStatus == ShippingStatus.EnShippingStatus.InTransit,

                ShippingStatus.EnShippingStatus.InTransit =>
                    newStatus == ShippingStatus.EnShippingStatus.Delivered,

                ShippingStatus.EnShippingStatus.Delivered =>
                    newStatus == ShippingStatus.EnShippingStatus.Returned,

                ShippingStatus.EnShippingStatus.Returned =>
                    false,

                _ => false
            };
        }


        private string GenerateTrackingNumber()
        {
            return $"TRK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }
}
