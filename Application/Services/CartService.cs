using Application.Common.Constants;
using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Features.Carts.DTOs;
using Application.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<AddToCartDto> _addToCartValidator;
        private readonly IValidator<UpdateCartItemQuantityDto> _updateQuantityValidator;
        private readonly ILogger<CartService> _logger;

        public CartService(ICustomerRepository customerRepository, IProductRepository productRepository, ICartRepository cartRepository, ICartItemRepository cartItemRepository,
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<AddToCartDto> addToCartValidator,
            IValidator<UpdateCartItemQuantityDto> updateQuantityValidator, ILogger<CartService> logger)
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _addToCartValidator = addToCartValidator;
            _updateQuantityValidator = updateQuantityValidator;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto request)
        {
            var validationResult = await _addToCartValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var customerId = await GetCurrentCustomerIdAsync();

                var product = await _productRepository.GetByIdAsync(request.ProductId);

                if (product is null)
                {
                    _logger.LogWarning("Cannot add product {ProductId} to cart because it was not found", request.ProductId);
                    throw new NotFoundException("Product not found.");
                }

                if (!product.IsActive)
                {
                    _logger.LogWarning("Cannot add inactive product {ProductId} to cart", request.ProductId);
                    throw new BadRequestException("Product is not active.");
                }

                if (request.Quantity > product.StockQuantity)
                {
                    _logger.LogWarning(
                        "Cannot add product {ProductId} to cart because requested quantity {RequestedQuantity} exceeds stock {StockQuantity}",
                        request.ProductId,
                        request.Quantity,
                        product.StockQuantity);
                    throw new BadRequestException("Quantity exceeds available stock.");
                }

                var cart = await _cartRepository.GetByCustomerIdWithCartItemsAsync(customerId);

                if (cart is null)
                {
                    cart = new Cart
                    {
                        CustomerId = customerId,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _cartRepository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                var existingItem = await _cartItemRepository.GetByCartAndProductAsync(cart.CartId, request.ProductId);

                if (existingItem is not null)
                {
                    var newQuantity = existingItem.Quantity + request.Quantity;

                    if (newQuantity > product.StockQuantity)
                    {
                        _logger.LogWarning(
                            "Cannot increase cart item for product {ProductId} to quantity {RequestedQuantity} because stock is {StockQuantity}",
                            request.ProductId,
                            newQuantity,
                            product.StockQuantity);
                        throw new BadRequestException("Quantity exceeds available stock.");
                    }

                    existingItem.Quantity = newQuantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    _cartItemRepository.Update(existingItem);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _cartItemRepository.AddAsync(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                _cartRepository.Update(cart);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Product {ProductId} added to cart {CartId} for customer {CustomerId}",
                    request.ProductId,
                    cart.CartId,
                    customerId);

                return await GetCartDtoAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product {ProductId} to cart", request.ProductId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ClearAsync()
        {
            var customerId = await GetCurrentCustomerIdAsync();

            var cart = await _cartRepository.GetByCustomerIdWithCartItemsAsync(customerId);

            if (cart is null)
                return;

            foreach (var item in cart.CartItems)
                _cartItemRepository.Delete(item);

            cart.UpdatedAt = DateTime.UtcNow;
            _cartRepository.Update(cart);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cart {CartId} cleared for customer {CustomerId}", cart.CartId, customerId);
        }

        public async Task<CartDto> GetMyCartAsync()
        {
            var customerId = await GetCurrentCustomerIdAsync();

            var cart = await _cartRepository.GetByCustomerIdWithCartItemsAsync(customerId);

            if (cart is null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow,
                };
                
                await _cartRepository.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetCartDtoAsync(customerId);
        }

        public async Task<CartDto> RemoveCartItemAsync(int cartItemId)
        {
            if (cartItemId <= 0)
            {
                _logger.LogWarning("Cannot remove cart item with invalid id {CartItemId}", cartItemId);
                throw new BadRequestException("cartItemId must be greater than 0.");
            }

            var customerId = await GetCurrentCustomerIdAsync();

            var cartItem = await _cartItemRepository.GetByIdWithCartAndProductAsync(cartItemId);

            if (cartItem is null)
            {
                _logger.LogWarning("Cannot remove cart item {CartItemId} because it was not found", cartItemId);
                throw new NotFoundException("Cart item not found.");
            }

            var isAdmin = _currentUserService.Roles.Contains(Roles.Admin);

            if (cartItem.Cart.CustomerId != customerId && !isAdmin)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to remove cart item {CartItemId} owned by another customer",
                    customerId,
                    cartItemId);
                throw new UnauthorizedAccessException("You cannot remove this cart item.");
            }

            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            _cartItemRepository.Delete(cartItem);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Cart item {CartItemId} removed from cart {CartId} for customer {CustomerId}",
                cartItemId,
                cartItem.CartId,
                customerId);

            return await GetCartDtoAsync(customerId);
        }

        public async Task<CartDto> UpdateCartItemQuantityAsync(int cartItemId, UpdateCartItemQuantityDto request)
        {
            var validationResult = await _updateQuantityValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            if (cartItemId <= 0)
            {
                _logger.LogWarning("Cannot update cart item with invalid id {CartItemId}", cartItemId);
                throw new BadRequestException("cartItemId must be greater than 0.");
            }

            int customerId = await GetCurrentCustomerIdAsync();

            var cartItem = await _cartItemRepository.GetByIdWithCartAndProductAsync(cartItemId);

            if (cartItem is null)
            {
                _logger.LogWarning("Cannot update cart item {CartItemId} because it was not found", cartItemId);
                throw new NotFoundException("Cart item not found.");
            }

            var isAdmin = _currentUserService.Roles.Contains(Roles.Admin);

            if (cartItem.Cart.CustomerId != customerId && !isAdmin)
            {
                _logger.LogWarning(
                    "Customer {CustomerId} attempted to update cart item {CartItemId} owned by another customer",
                    customerId,
                    cartItemId);
                throw new UnauthorizedAccessException("You cannot update this cart item.");
            }

            if (request.Quantity > cartItem.Product.StockQuantity)
            {
                _logger.LogWarning(
                    "Cannot update cart item {CartItemId} to quantity {RequestedQuantity} because stock is {StockQuantity}",
                    cartItemId,
                    request.Quantity,
                    cartItem.Product.StockQuantity);
                throw new BadRequestException("Quantity exceeds available stock.");
            }

            cartItem.Quantity = request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            _cartItemRepository.Update(cartItem);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Cart item {CartItemId} quantity updated to {Quantity} for customer {CustomerId}",
                cartItemId,
                request.Quantity,
                customerId);

            return await GetCartDtoAsync(customerId);
        }

        private async Task<int> GetCurrentCustomerIdAsync()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to access a customer cart");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (_currentUserService.UserId is null)
            {
                _logger.LogWarning("Authenticated user is missing a user id claim while accessing a customer cart");
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

        private async Task<CartDto> GetCartDtoAsync(int customerId)
        {
            var cart = await _cartRepository.GetByCustomerIdWithItemsProductsAndImagesAsync(customerId);

            if (cart is null)
            {
                _logger.LogWarning("Cart was not found for customer {CustomerId}", customerId);
                throw new NotFoundException("Cart not found.");
            }

            return cart.ToDto();
        }
    }
}
