using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Features.ProductImages.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<AddProductImageDto> _addValidator;
        private readonly IValidator<UpdateProductImageDto> _updateValidator;
        private readonly ILogger<ProductImageService> _logger;

        public ProductImageService(IProductImageRepository productImageRepository, IProductRepository productRepository,
         IFileStorageService fileStorageService, IUnitOfWork unitOfWork, IValidator<AddProductImageDto> addValidator,
          IValidator<UpdateProductImageDto> updateValidator, ILogger<ProductImageService> logger)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<ProductImageDto> AddAsync(AddProductImageDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var validationResult = await _addValidator.ValidateAsync(request);
                validationResult.ThrowIfValidationFails();

                var productExists = await _productRepository.ExistsActiveProductAsync(request.ProductId);

                if (!productExists)
                {
                    _logger.LogWarning(
                        "Cannot add image because active product {ProductId} was not found",
                        request.ProductId);
                    throw new NotFoundException("Product not found.");
                }

                if (request.IsMain)
                    await _productImageRepository.ClearMainImageAsync(request.ProductId);
                else
                    request.IsMain = !await _productImageRepository.ProductHasMainImageAsync(request.ProductId);


                var imageUrl = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName, request.ContentType,
                 "products");

                var sortOrder = request.SortOrder
                    ?? await _productImageRepository.GetNextSortOrderAsync(request.ProductId);

                var image = new ProductImage
                {
                    ProductId = request.ProductId,
                    ImageUrl = imageUrl,
                    AltText = request.AltText,
                    IsMain = request.IsMain,
                    SortOrder = sortOrder,
                    CreatedAt = DateTime.UtcNow
                };

                await _productImageRepository.AddAsync(image);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Product image {ProductImageId} added to product {ProductId}",
                    image.ProductImageId,
                    request.ProductId);

                return image.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add image to product {ProductId}", request.ProductId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int productId, int productImageId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot delete product image for invalid product id {ProductId}", productId);
                throw new BadRequestException("ProductId must be greater than 0.");
            }

            if (productImageId <= 0)
            {
                _logger.LogWarning("Cannot delete product image with invalid id {ProductImageId}", productImageId);
                throw new BadRequestException("ProductImageId must be greater than 0.");
            }

            string? imageUrlToDelete = null;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var image = await _productImageRepository.GetByIdAndProductIdAsync(
                    productImageId,
                    productId);

                if (image is null)
                {
                    _logger.LogWarning(
                        "Cannot delete product image {ProductImageId} because it was not found for product {ProductId}",
                        productImageId,
                        productId);
                    throw new NotFoundException("Product image not found.");
                }

                var wasMain = image.IsMain;
                imageUrlToDelete = image.ImageUrl;

                _productImageRepository.Delete(image);
                await _unitOfWork.SaveChangesAsync();

                if (wasMain)
                {
                    var remainingImages = await _productImageRepository.GetByProductIdAsync(productId);
                    var firstImage = remainingImages.FirstOrDefault();

                    if (firstImage is not null)
                    {
                        firstImage.IsMain = true;
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete product image {ProductImageId} from product {ProductId}",
                    productImageId,
                    productId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            if (!string.IsNullOrWhiteSpace(imageUrlToDelete))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(imageUrlToDelete);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to delete stored file for product image {ProductImageId} from product {ProductId}",
                        productImageId,
                        productId);
                    throw;
                }
            }

            _logger.LogInformation(
                "Product image {ProductImageId} deleted from product {ProductId}",
                productImageId,
                productId);
        }

        public async Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot get images for invalid product id {ProductId}", productId);
                throw new BadRequestException("ProductId must be greater than 0.");
            }

            var productExists = await _productRepository.ExistsActiveProductAsync(productId);

            if (!productExists)
            {
                _logger.LogWarning("Cannot get images because active product {ProductId} was not found", productId);
                throw new NotFoundException("Product not found.");
            }

            var images = await _productImageRepository.GetByProductIdAsync(productId);

            return images.ToDtoList();
        }

        public async Task SetMainAsync(int productId, int productImageId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot set main image for invalid product id {ProductId}", productId);
                throw new BadRequestException("ProductId must be greater than 0.");
            }

            if (productImageId <= 0)
            {
                _logger.LogWarning("Cannot set main product image with invalid id {ProductImageId}", productImageId);
                throw new BadRequestException("ProductImageId must be greater than 0.");
            }

            var image = await _productImageRepository.GetByIdAndProductIdAsync(productImageId, productId);

            if (image is null)
            {
                _logger.LogWarning(
                    "Cannot set main image because product image {ProductImageId} was not found for product {ProductId}",
                    productImageId,
                    productId);
                throw new NotFoundException("Product image not found");
            }

            await _productImageRepository.ClearMainImageAsync(productId);

            image.IsMain = true;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Product image {ProductImageId} set as main image for product {ProductId}",
                productImageId,
                productId);
        }

        public async Task<ProductImageDto> UpdateAsync(UpdateProductImageDto request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            var image = await _productImageRepository.GetByIdAndProductIdAsync(request.ProductImageId, request.ProductId);

            if (image is null)
            {
                _logger.LogWarning(
                    "Cannot update product image {ProductImageId} because it was not found for product {ProductId}",
                    request.ProductImageId,
                    request.ProductId);
                throw new NotFoundException("Product image not found.");
            }

            if (request.IsMain)
                await _productImageRepository.ClearMainImageAsync(request.ProductId);

            if (request.FileStream is not null)
            {
                if (string.IsNullOrWhiteSpace(request.FileName))
                {
                    _logger.LogWarning(
                        "Cannot replace file for product image {ProductImageId} because file name is missing",
                        request.ProductImageId);
                    throw new BadRequestException("File name is required.");
                }

                if (string.IsNullOrWhiteSpace(request.ContentType))
                {
                    _logger.LogWarning(
                        "Cannot replace file for product image {ProductImageId} because content type is missing",
                        request.ProductImageId);
                    throw new BadRequestException("Content type is required.");
                }

                var oldImageUrl = image.ImageUrl;

                try
                {
                    var newImageUrl = await _fileStorageService.SaveFileAsync(
                        request.FileStream,
                        request.FileName,
                        request.ContentType,
                        "products");

                    image.ImageUrl = newImageUrl;
                    await _fileStorageService.DeleteFileAsync(oldImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to replace stored file for product image {ProductImageId} on product {ProductId}",
                        request.ProductImageId,
                        request.ProductId);
                    throw;
                }
            }

            image.AltText = request.AltText;
            image.IsMain = request.IsMain;
            image.SortOrder = request.SortOrder;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Product image {ProductImageId} updated for product {ProductId}",
                request.ProductImageId,
                request.ProductId);

            return image.ToDto();
        }
    }
}
