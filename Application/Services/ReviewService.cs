using Application.Common.Constants;
using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Features.Reviews.DTOs;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateReviewDto> _createReviewDtoValidator;
        private readonly IValidator<PatchReviewDto> _updateReviewDtoValidator;
        private readonly ILogger<ReviewService> _logger;
        private readonly IAuditService _auditService;

        public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository, IOrderRepository orderRepository,
            ICustomerRepository customerRepository, ICurrentUserService currentUserService, IValidator<CreateReviewDto> createReviewDtoValidator,
            IValidator<PatchReviewDto> updateReviewDtoValidator, IUnitOfWork unitOfWork, ILogger<ReviewService> logger,
            IAuditService auditService)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
            _createReviewDtoValidator = createReviewDtoValidator;
            _updateReviewDtoValidator = updateReviewDtoValidator;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<ReviewDto> AdminApproveReviewAsync(int reviewId)
        {
            if (reviewId <= 0)
            {
                _logger.LogWarning("Cannot approve review with invalid id {ReviewId}", reviewId);
                throw new BadRequestException("review id must be greater than 0.");
            }

            var productReview = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);

            if (productReview is null)
            {
                _logger.LogWarning("Cannot approve review {ReviewId} because it was not found", reviewId);
                throw new NotFoundException("product review is not found.");
            }

            var wasApproved = productReview.IsApproved;

            productReview.IsApproved = true;
            productReview.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.ReviewApproved,
                AuditEntities.Review,
                reviewId,
                oldValues: JsonSerializer.Serialize(new { IsApproved = wasApproved }),
                newValues: JsonSerializer.Serialize(new
                {
                    IsApproved = true,
                    productReview.ProductId
                }));

            _logger.LogInformation(
                "Review {ReviewId} approved for product {ProductId}",
                reviewId,
                productReview.ProductId);

            return productReview.ToDto();
        }

        public async Task<ReviewDto> CreateReviewAsync(int productId, CreateReviewDto request)
        {
            var validationrResult = await _createReviewDtoValidator.ValidateAsync(request);
            validationrResult.ThrowIfValidationFails();

            if (productId <= 0)
            {
                _logger.LogWarning("Cannot create review for invalid product id {ProductId}", productId);
                throw new BadRequestException("product id must be greater than 0.");
            }

            var existsProduct = await _productRepository.ExistsActiveProductAsync(productId);

            if (!existsProduct)
            {
                _logger.LogWarning("Cannot create review because active product {ProductId} was not found", productId);
                throw new NotFoundException("Product is not found.");
            }

            var customerId = await GetCurrentCustomerIdAsync();

            var productHasReview = await _reviewRepository.ExistsByCustomerAndProductAsync(customerId, productId);

            if (productHasReview)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to create a duplicate review for product {ProductId}",
                    customerId,
                    productId);
                throw new BadRequestException("you already reviewed this product, you can only review one time.");
            }

            var purchasedProduct = await _orderRepository.HasCustomerPurchasedProductAsync(customerId, productId);

            if (!purchasedProduct)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to review unpurchased product {ProductId}",
                    customerId,
                    productId);
                throw new BadRequestException("cannot review product you didn`t bought.");
            }

            var review = new Review
            {
                CustomerId = customerId,
                ProductId = productId,
                Rating = request.Rating,
                Comment = request.Comment,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.ReviewCreated,
                AuditEntities.Review,
                review.ReviewId,
                newValues: JsonSerializer.Serialize(new
                {
                    ProductId = productId,
                    CustomerId = customerId,
                    review.Rating,
                    review.IsApproved
                }));

            _logger.LogInformation(
                "Review {ReviewId} created for product {ProductId} by customer {CustomerId}",
                review.ReviewId,
                productId,
                customerId);

            return review.ToDto();
        }

        public async Task DeleteAsync(int reviewId)
        {
            if (reviewId <= 0)
            {
                _logger.LogWarning("Cannot delete review with invalid id {ReviewId}", reviewId);
                throw new BadRequestException("review id must be greater than 0.");
            }

            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review is null)
            {
                _logger.LogWarning("Cannot delete review {ReviewId} because it was not found", reviewId);
                throw new NotFoundException("review is not found.");
            }

            bool isAdmin = _currentUserService.Roles.Contains(Roles.Admin);
            bool isSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

            if (!isAdmin && !isSuperAdmin)
            {
                var customerId = await GetCurrentCustomerIdAsync();

                if (customerId != review.CustomerId)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to delete review {ReviewId} owned by another customer",
                        customerId,
                        reviewId);
                    throw new ForbiddenException("You can delete only your own review.");
                }
            }

            _reviewRepository.Delete(review);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Review {ReviewId} deleted for product {ProductId}",
                reviewId,
                review.ProductId);
        }

        public async Task<IEnumerable<ReviewDto>> GetAllByProductIdAsync(int productId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot get reviews for invalid product id {ProductId}", productId);
                throw new BadRequestException("product id must be greater than 0.");
            }

            var productReviews = await _reviewRepository.GetApprovedByProductIdAsync(productId);

            return productReviews.ToDtoList();
        }

        public async Task<ReviewDto> GetByReviewIdWithProductDetailsAsync(int reviewId)
        {
            if (reviewId <= 0)
            {
                _logger.LogWarning("Cannot get review with invalid id {ReviewId}", reviewId);
                throw new BadRequestException("review id must be greater than 0.");
            }

            var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);

            if (review is null)
            {
                _logger.LogWarning("Review {ReviewId} was not found", reviewId);
                throw new NotFoundException("review is not found.");
            }

            return review.ToDto();
        }

        public async Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync()
        {
            var pendingReviews = await _reviewRepository.GetPendingReviewsAsync();

            return pendingReviews.ToDtoList();
        }

        public async Task<ReviewDto> PatchReviewAsync(int reviewId, PatchReviewDto request)
        {
            var validationResult = await _updateReviewDtoValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            if (reviewId <= 0)
            {
                _logger.LogWarning("Cannot update review with invalid id {ReviewId}", reviewId);
                throw new BadRequestException("review id must be greater than 0.");
            }

            var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);

            if (review is null)
            {
                _logger.LogWarning("Cannot update review {ReviewId} because it was not found", reviewId);
                throw new NotFoundException("review is not found.");
            }

            bool isAdmin = _currentUserService.Roles.Contains(Roles.Admin);
            bool isSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

            if (!isAdmin && !isSuperAdmin)
            {
                var customerId = await GetCurrentCustomerIdAsync();

                if (customerId != review.CustomerId)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to update review {ReviewId} owned by another customer",
                        customerId,
                        reviewId);
                    throw new ForbiddenException("You can update only your own review.");
                }
            }

            if (request.Comment is not null)
                review.Comment = request.Comment;

            if (request.Rating.HasValue)
                review.Rating = request.Rating.Value;

            if (!isAdmin && !isSuperAdmin)
            {
                review.IsApproved = false;
            }

            review.UpdatedAt = DateTime.UtcNow;

            _reviewRepository.Update(review);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Review {ReviewId} updated for product {ProductId}",
                reviewId,
                review.ProductId);

            return review.ToDto();
        }


        private async Task<int> GetCurrentCustomerIdAsync()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to access customer reviews");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (_currentUserService.UserId is null)
            {
                _logger.LogWarning("Authenticated user is missing a user id claim while accessing customer reviews");
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
    }
}
