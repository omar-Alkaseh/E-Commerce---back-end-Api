using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Constants;
using Application.Common.Validation;
using Application.Features.Products.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;
        private readonly IValidator<PatchProductDto> _patchValidator;
        private readonly ISkuGenerator _skuGenerator;
        private readonly ISlugGenerator _slugGenerator;
        private readonly ILogger<ProductService> _logger;
        private readonly IAuditService _auditService;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork,
            IValidator<CreateProductDto> createValidator, IValidator<UpdateProductDto> updateValidator, ISkuGenerator skuGenerator,
            ISlugGenerator slugGenerator, IValidator<PatchProductDto> patchValidator, ILogger<ProductService> logger,
            IAuditService auditService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _skuGenerator = skuGenerator;
            _slugGenerator = slugGenerator;
            _patchValidator = patchValidator;
            _logger = logger;
            _auditService = auditService;
        }


        private async Task<string> GenerateUniqueSlugAsync(string productName)
        {
            var baseSlug = _slugGenerator.GenerateSlug(productName);

            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "product";


            var slug = baseSlug;
            var counter = 1;

            while (await _productRepository.SlugExistsAsync(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private async Task<string> GenerateUniqueSlugAsync(string productName, int productId)
        {
            var baseSlug = _slugGenerator.GenerateSlug(productName);

            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "product";


            var slug = baseSlug;
            var counter = 1;

            while (await _productRepository.SlugExistsForOtherProductAsync(slug, productId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

            if (!categoryExists)
            {
                _logger.LogWarning(
                    "Cannot create product because category {CategoryId} was not found",
                    request.CategoryId);
                throw new NotFoundException("Category not found.");
            }

            var sku = string.IsNullOrWhiteSpace(request.Sku)
            ? _skuGenerator.GenerateSku(request.ProductName, request.CategoryId)
            : request.Sku.Trim().ToUpperInvariant();

            if (await _productRepository.SkuExistsAsync(sku))
            {
                _logger.LogWarning("Cannot create product because SKU already exists");
                throw new BadRequestException("SKU already exists.");
            }

            var slug = await GenerateUniqueSlugAsync(request.ProductName);

            var product = new Product
            {
                Name = request.ProductName,
                Description = request.Description,
                Sku = sku,
                Slug = slug,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                IsFeatured = request.IsFeatured,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.ProductCreated,
                AuditEntities.Product,
                product.ProductId,
                newValues: JsonSerializer.Serialize(new
                {
                    product.CategoryId,
                    product.IsActive,
                    product.IsFeatured
                }));

            _logger.LogInformation(
                "Product {ProductId} created in category {CategoryId}",
                product.ProductId,
                product.CategoryId);

            return product.ToDto();
        }

        public async Task DeleteAsync(int productId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot delete product with invalid id {ProductId}", productId);
                throw new BadRequestException("ProductId must be greater than 0.");
            }

            var product = await _productRepository.GetByIdAsync(productId);

            if (product is null)
            {
                _logger.LogWarning("Cannot delete product {ProductId} because it was not found", productId);
                throw new NotFoundException("Product not found.");
            }

            _productRepository.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} deleted", productId);
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            var activeProducts = await _productRepository.GetActiveProductsAsync();

            return activeProducts.ToDtoList();
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                _logger.LogWarning("Cannot get products for invalid category id {CategoryId}", categoryId);
                throw new BadRequestException("CategoryId must be greater than 0.");
            }

            var productsInCategory = await _productRepository.GetByCategoryIdAsync(categoryId);

            return productsInCategory.ToDtoList();
        }

        public async Task<ProductDto?> GetByIdWithImagesAsync(int productId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot get product with invalid id {ProductId}", productId);
                throw new BadRequestException("ProductId must be greater than 0.");
            }

            var product = await _productRepository.GetByIdWithImagesAsync(productId);

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} was not found", productId);
                throw new NotFoundException("Product not found.");
            }

            return product.ToDto();
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedAsync()
        {
            var featureProducts = await _productRepository.GetFeaturedAsync();

            return featureProducts.ToDtoList();
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogWarning("Product search rejected because the search term was empty");
                throw new BadRequestException("Invalid search term.");
            }

            var searchProducts = await _productRepository.SearchAsync(searchTerm.Trim());

            return searchProducts.ToDtoList();
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductDto request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            var product = await _productRepository.GetByIdWithImagesAsync(request.ProductId);

            if (product is null)
            {
                _logger.LogWarning("Cannot update product {ProductId} because it was not found", request.ProductId);
                throw new NotFoundException("Product not found.");
            }

            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

            if (!categoryExists)
            {
                _logger.LogWarning(
                    "Cannot update product {ProductId} because category {CategoryId} was not found",
                    request.ProductId,
                    request.CategoryId);
                throw new NotFoundException("Category not found.");
            }


            var sku = string.IsNullOrWhiteSpace(request.Sku)
                ? product.Sku
                : request.Sku.Trim().ToUpperInvariant();


            if (await _productRepository.SkuExistsForOtherProductAsync(sku, request.ProductId))
            {
                _logger.LogWarning("Cannot update product {ProductId} because SKU is already in use", request.ProductId);
                throw new BadRequestException("This SKU is used by another product.");
            }

            var slug = await GenerateUniqueSlugAsync(request.ProductName, request.ProductId);

            product.Sku = sku;
            product.Slug = slug;
            product.IsFeatured = request.IsFeatured;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            product.Description = request.Description;
            product.Name = request.ProductName;
            product.DiscountPrice = request.DiscountPrice;
            product.Price = request.Price;
            product.IsActive = request.IsActive;
            product.StockQuantity = request.StockQuantity;

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.ProductUpdated,
                AuditEntities.Product,
                request.ProductId,
                newValues: JsonSerializer.Serialize(new
                {
                    product.CategoryId,
                    product.IsActive,
                    product.IsFeatured,
                    product.StockQuantity
                }));

            _logger.LogInformation("Product {ProductId} updated", request.ProductId);

            return product.ToDto();
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllWithMainImageAsync();

            return products.ToDtoList();
        }

        public async Task<ProductDto?> GetByIdAsync(int productId)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("Cannot get product with invalid id {ProductId}", productId);
                throw new BadRequestException("ProductId must be greater than 0.");
            }

            var product = await _productRepository.GetByIdAsync(productId);

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} was not found", productId);
                throw new NotFoundException("Product not found.");
            }

            return product.ToDto();
        }

        public async Task<ProductDto> PatchAsync(PatchProductDto request)
        {
            var validationResult = await _patchValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            var product = await _productRepository.GetByIdWithImagesAsync(request.ProductId);

            if (product is null)
            {
                _logger.LogWarning("Cannot patch product {ProductId} because it was not found", request.ProductId);
                throw new NotFoundException("Product not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.ProductName))
            {
                product.Name = request.ProductName;
                product.Slug = await GenerateUniqueSlugAsync(request.ProductName, request.ProductId);
            }

            if (request.Description is not null)
                product.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.Sku))
            {
                var sku = request.Sku.Trim().ToUpperInvariant();

                if (await _productRepository.SkuExistsForOtherProductAsync(sku, request.ProductId))
                {
                    _logger.LogWarning("Cannot patch product {ProductId} because SKU is already in use", request.ProductId);
                    throw new BadRequestException("This SKU is used by another product.");
                }

                product.Sku = sku;
            }

            if (request.Price.HasValue)
                product.Price = request.Price.Value;

            if (request.ClearDiscountPrice)
                product.DiscountPrice = null;
            else if (request.DiscountPrice.HasValue)
                product.DiscountPrice = request.DiscountPrice.Value;

            if (product.DiscountPrice.HasValue && product.DiscountPrice.Value >= product.Price)
            {
                _logger.LogWarning(
                    "Cannot patch product {ProductId} because discount price is not less than price",
                    request.ProductId);
                throw new BadRequestException("Discount price must be less than price.");
            }

            if (request.StockQuantity.HasValue)
                product.StockQuantity = request.StockQuantity.Value;

            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId.Value);

                if (!categoryExists)
                {
                    _logger.LogWarning(
                        "Cannot patch product {ProductId} because category {CategoryId} was not found",
                        request.ProductId,
                        request.CategoryId.Value);
                    throw new NotFoundException("Category not found.");
                }

                product.CategoryId = request.CategoryId.Value;
            }

            if (request.IsFeatured.HasValue)
                product.IsFeatured = request.IsFeatured.Value;

            if  (request.IsActive.HasValue)
                product.IsActive = request.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.ProductUpdated,
                AuditEntities.Product,
                request.ProductId,
                newValues: JsonSerializer.Serialize(new
                {
                    product.CategoryId,
                    product.IsActive,
                    product.IsFeatured,
                    product.StockQuantity
                }));

            _logger.LogInformation("Product {ProductId} patched", request.ProductId);

            return product.ToDto();
        }

        public async Task<PaginatedResult<ProductDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                _logger.LogWarning("Product page request rejected for invalid page number {PageNumber}", pageNumber);
                throw new BadRequestException("PageNumber must be greater than 0.");
            }

            if (pageSize < 1)
            {
                _logger.LogWarning("Product page request rejected for invalid page size {PageSize}", pageSize);
                throw new BadRequestException("PageSize must be greater than 0.");
            }

            if (pageSize > 100)
                pageSize = 100;

            var totalCount = await _productRepository.CountAsync();

            var products = await _productRepository.GetPagedWithMainImageAsync(pageNumber, pageSize);

            return new PaginatedResult<ProductDto>
            {
                Items = products.ToDtoList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<ProductDto>> GetProductsByMinimumRatingAsync(int minRating)
        {
            if (minRating <= 0 || minRating >=6)
            {
                _logger.LogWarning("Product rating filter rejected for invalid minimum rating {MinimumRating}", minRating);
                throw new BadRequestException("minRating must be between 1 and 5.");
            }

            var products = await _productRepository.GetProductsByMinimumRatingAsync(minRating);

            return products.ToDtoList();
        }

        public async Task<List<ProductDto>> GetTopRatedProductsAsync()
        {
            var products = await _productRepository.GetTopRatedProductsAsync();

            return products.ToDtoList();
        }
    }
}
