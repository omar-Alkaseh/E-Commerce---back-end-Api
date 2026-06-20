using Application.Common.Validation;
using Application.Features.Payments.DTOs;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using FluentValidation;
using Application.Common.Exceptions;
using Domain.Enums;
using Domain.Entities;
using Application.Mappings;
using Application.Common.Constants;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IValidator<CreatePaymentDto> _createPaymentValidator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<PaymentService> _logger;
        private readonly IAuditService _auditService;

        public PaymentService(IPaymentRepository paymentRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork,
            IOrderRepository orderRepository, IValidator<CreatePaymentDto> createPaymentValidator, ICustomerRepository customerRepository,
            ILogger<PaymentService> logger, IAuditService auditService)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _createPaymentValidator = createPaymentValidator;
            _customerRepository = customerRepository;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<PaymentDto> CreatePaymentAsync(int orderId, CreatePaymentDto paymentMethod)
        {
            var validationResult = await _createPaymentValidator.ValidateAsync(paymentMethod);
            validationResult.ThrowIfValidationFails();

            if (orderId <= 0)
            {
                _logger.LogWarning("Cannot create payment for invalid order id {OrderId}", orderId);
                throw new BadRequestException("OrderId must be greater than 0.");
            }

            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

            if (order is null)
            {
                _logger.LogWarning("Cannot create payment because order {OrderId} was not found", orderId);
                throw new NotFoundException("Order not found.");
            }

            var customerId = await GetCurrentCustomerId();

            if (order.CustomerId != customerId)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to pay for order {OrderId} owned by another customer",
                    customerId,
                    orderId);
                throw new ForbiddenException("You cannot pay for this order.");
            }

            if (await _paymentRepository.HasActivePaymentForOrderAsync(orderId))
            {
                _logger.LogWarning("Cannot create duplicate active payment for order {OrderId}", orderId);
                throw new ConflictException("Payment already exists for this order.");
            }

            if ((OrderStatus.EnOrderStatus)order.OrderStatus == OrderStatus.EnOrderStatus.Cancelled)
            {
                _logger.LogWarning("Cannot create payment for cancelled order {OrderId}", orderId);
                throw new BadRequestException("Cannot create payment for cancelled order.");
            }

            var payment = new Payment
            {
                OrderId = orderId,
                PaymentMethod = (byte)paymentMethod.PaymentMethod,
                PaymentStatus = (byte)PaymentStatus.EnPaymentStatus.Pending,
                Amount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow
            };

            if (paymentMethod.PaymentMethod == PaymentMethod.EnPaymentMethod.CashOnDelivery)
                payment.TransactionId = $"COD-{DateTime.UtcNow:yyyyMMddHHmmss}-{GenerateShortId()}";

            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.PaymentCreated,
                AuditEntities.Payment,
                payment.PaymentId,
                newValues: JsonSerializer.Serialize(new
                {
                    OrderId = orderId,
                    PaymentMethod = paymentMethod.PaymentMethod,
                    Status = PaymentStatus.EnPaymentStatus.Pending
                }));

            _logger.LogInformation(
                "Payment {PaymentId} created for order {OrderId} using method {PaymentMethod}",
                payment.PaymentId,
                orderId,
                paymentMethod.PaymentMethod);

            return payment.ToDto();
        }

        public async Task<IEnumerable<PaymentDto>> GetMyPaymentsAsync()
        {
            var customerId = await GetCurrentCustomerId();

            var myPayments = await _paymentRepository.GetMyPaymentsAsync(customerId);

            return myPayments.ToDtoList();
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();

            return payments.ToDtoList();
        }

        public async Task<IEnumerable<PaymentDto>> GetByOrderIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                _logger.LogWarning("Cannot get payments for invalid order id {OrderId}", orderId);
                throw new BadRequestException("OrderId must be greater than 0.");
            }

            var payments = (await _paymentRepository.GetByOrderIdAsync(orderId)).ToList();

            if (!payments.Any())
            {
                _logger.LogWarning("No payments were found for order {OrderId}", orderId);
                throw new NotFoundException("Payment not found.");
            }

            var customerId = await GetCurrentCustomerId();
            bool isAdmin = _currentUserService.Roles.Contains(Roles.Admin);

            foreach (var payment in payments)
            {
                if (payment.Order.CustomerId != customerId && !isAdmin)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to access payment {PaymentId} for order {OrderId}",
                        customerId,
                        payment.PaymentId,
                        orderId);
                    throw new ForbiddenException("You cannot access this payment.");
                }
            }

            return payments.ToDtoList();
        }

        public async Task<PaymentDto> ProcessFakePaymentAsync(int paymentId, bool isSuccess, string? failureReason)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = await _paymentRepository.GetByIdWithOrderAndShipment(paymentId);

                if (payment is null)
                {
                    _logger.LogWarning("Cannot process payment {PaymentId} because it was not found", paymentId);
                    throw new NotFoundException("Payment not found.");
                }

                var customerId = await GetCurrentCustomerId();

                bool isAdmin = _currentUserService.Roles.Contains(Roles.Admin);
                bool isSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

                if (payment.Order.CustomerId != customerId && !isAdmin && !isSuperAdmin)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to process payment {PaymentId} owned by another customer",
                        customerId,
                        paymentId);
                    throw new ForbiddenException("You cannot process this payment.");
                }

                if ((PaymentStatus.EnPaymentStatus)payment.PaymentStatus != PaymentStatus.EnPaymentStatus.Pending)
                {
                    _logger.LogWarning(
                        "Cannot process payment {PaymentId} because its status is {PaymentStatus}",
                        paymentId,
                        (PaymentStatus.EnPaymentStatus)payment.PaymentStatus);
                    throw new BadRequestException("Only pending payments can be processed.");
                }

                if ((PaymentMethod.EnPaymentMethod)payment.PaymentMethod == PaymentMethod.EnPaymentMethod.CashOnDelivery)
                {
                    _logger.LogWarning("Cannot process cash on delivery payment {PaymentId} online", paymentId);
                    throw new BadRequestException("Cash on delivery cannot be processed online.");
                }

                if (isSuccess)
                {
                    payment.PaymentStatus = (byte)PaymentStatus.EnPaymentStatus.Paid;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{GenerateShortId()}";


                    payment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Processing;
                    payment.Order.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    payment.PaymentStatus = (byte)PaymentStatus.EnPaymentStatus.Failed;
                    payment.FailedAt = DateTime.UtcNow;
                    payment.FailureReason = string.IsNullOrWhiteSpace(failureReason) ? "Payment failed." : failureReason;
                }

                _paymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditService.LogAsync(
                    AuditActions.PaymentProcessed,
                    AuditEntities.Payment,
                    paymentId,
                    oldValues: JsonSerializer.Serialize(new { Status = PaymentStatus.EnPaymentStatus.Pending }),
                    newValues: JsonSerializer.Serialize(new
                    {
                        Status = (PaymentStatus.EnPaymentStatus)payment.PaymentStatus,
                        payment.OrderId
                    }));

                _logger.LogInformation(
                    "Payment {PaymentId} processed for order {OrderId} with status {PaymentStatus}",
                    paymentId,
                    payment.OrderId,
                    (PaymentStatus.EnPaymentStatus)payment.PaymentStatus);

                return payment.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process payment {PaymentId}", paymentId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaymentDto> RefundAsync(int paymentId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = await _paymentRepository.GetByIdWithOrderAndShipment(paymentId);

                if (payment is null)
                {
                    _logger.LogWarning("Cannot refund payment {PaymentId} because it was not found", paymentId);
                    throw new NotFoundException("Payment not found.");
                }

                bool isAdmin = _currentUserService.Roles.Contains(Roles.Admin);
                bool isSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

                if (!isAdmin && !isSuperAdmin)
                {
                    _logger.LogWarning("Unauthorized refund attempt for payment {PaymentId}", paymentId);
                    throw new ForbiddenException("Only admin can refund payments.");
                }

                if (payment.PaymentStatus != (byte)PaymentStatus.EnPaymentStatus.Paid)
                {
                    _logger.LogWarning(
                        "Cannot refund payment {PaymentId} because its status is {PaymentStatus}",
                        paymentId,
                        (PaymentStatus.EnPaymentStatus)payment.PaymentStatus);
                    throw new ConflictException("Only paid payments can be refunded.");
                }

                payment.PaymentStatus = (byte)PaymentStatus.EnPaymentStatus.Refunded;
                payment.RefundedAt = DateTime.UtcNow;

                if (payment.Order.OrderStatus == (byte)OrderStatus.EnOrderStatus.Delivered)
                {
                    payment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Returned;
                }
                else
                {
                    payment.Order.OrderStatus = (byte)OrderStatus.EnOrderStatus.Cancelled;
                }

                payment.Order.UpdatedAt = DateTime.UtcNow;

                _paymentRepository.Update(payment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _auditService.LogAsync(
                    AuditActions.PaymentRefunded,
                    AuditEntities.Payment,
                    paymentId,
                    oldValues: JsonSerializer.Serialize(new { Status = PaymentStatus.EnPaymentStatus.Paid }),
                    newValues: JsonSerializer.Serialize(new
                    {
                        Status = PaymentStatus.EnPaymentStatus.Refunded,
                        payment.OrderId
                    }));

                _logger.LogInformation(
                    "Payment {PaymentId} refunded for order {OrderId}",
                    paymentId,
                    payment.OrderId);

                return payment.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refund payment {PaymentId}", paymentId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }



     
        private async Task<int> GetCurrentCustomerId()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to access customer payments");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (_currentUserService.UserId is null)
            {
                _logger.LogWarning("Authenticated user is missing a user id claim while accessing customer payments");
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

        private static string GenerateShortId()
        {
            return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        }
    }
}
