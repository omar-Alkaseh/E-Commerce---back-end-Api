using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Features.Categories.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateCategoryDto> createValidator,
            IValidator<UpdateCategoryDto> updateValidator,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            if (await _categoryRepository.NameExistsAsync(request.Name))
            {
                _logger.LogWarning("Cannot create category because the name already exists");
                throw new BadRequestException("Category name already exists.");
            }

            if (await _categoryRepository.SlugExistsAsync(request.Slug))
            {
                _logger.LogWarning("Cannot create category because the slug already exists");
                throw new BadRequestException("Category slug already exists.");
            }

            if (request.ParentCategoryId is not null)
            {
                var parentExists = await _categoryRepository.ExistsAsync(request.ParentCategoryId.Value);

                if (!parentExists)
                {
                    _logger.LogWarning(
                        "Cannot create category because parent category {ParentCategoryId} was not found",
                        request.ParentCategoryId.Value);
                    throw new NotFoundException("Parent category not found.");
                }
            }

            var category = new Category
            {
                Name = request.Name.Trim(),
                Slug = request.Slug.Trim().ToLower(),
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Category {CategoryId} created with parent category {ParentCategoryId}",
                category.CategoryId,
                category.ParentCategoryId);

            return category.MapToDto();
        }

        public async Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryDto request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            if (categoryId <= 0)
            {
                _logger.LogWarning("Cannot update category with invalid id {CategoryId}", categoryId);
                throw new BadRequestException("CategoryId must be greater than 0.");
            }

            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category is null)
            {
                _logger.LogWarning("Cannot update category {CategoryId} because it was not found", categoryId);
                throw new NotFoundException("Category not found.");
            }

            if (await _categoryRepository.NameExistsForOtherCategoryAsync(categoryId, request.Name))
            {
                _logger.LogWarning("Cannot update category {CategoryId} because the name already exists", categoryId);
                throw new BadRequestException("Category name already exists.");
            }

            if (await _categoryRepository.SlugExistsForOtherCategoryAsync(categoryId, request.Slug))
            {
                _logger.LogWarning("Cannot update category {CategoryId} because the slug already exists", categoryId);
                throw new BadRequestException("Category slug already exists.");
            }

            if (request.ParentCategoryId == categoryId)
            {
                _logger.LogWarning("Category {CategoryId} cannot be its own parent", categoryId);
                throw new BadRequestException("Category cannot be parent of itself.");
            }

            if (request.ParentCategoryId is not null)
            {
                var parentExists = await _categoryRepository.ExistsAsync(request.ParentCategoryId.Value);

                if (!parentExists)
                {
                    _logger.LogWarning(
                        "Cannot update category {CategoryId} because parent category {ParentCategoryId} was not found",
                        categoryId,
                        request.ParentCategoryId.Value);
                    throw new NotFoundException("Parent category not found.");
                }
            }

            category.Name = request.Name.Trim();
            category.Slug = request.Slug.Trim().ToLower();
            category.Description = request.Description;
            category.ParentCategoryId = request.ParentCategoryId;
            category.IsActive = request.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} updated", categoryId);

            return category.MapToDto();
        }

        public async Task DeleteAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                _logger.LogWarning("Cannot delete category with invalid id {CategoryId}", categoryId);
                throw new BadRequestException("CategoryId must be greater than 0.");
            }

            var category = await _categoryRepository.GetByIdWithChildrenAsync(categoryId);

            if (category is null)
            {
                _logger.LogWarning("Cannot delete category {CategoryId} because it was not found", categoryId);
                throw new NotFoundException("Category not found.");
            }

            if (category.InverseParentCategory.Any())
            {
                _logger.LogWarning("Cannot delete category {CategoryId} because it has subcategories", categoryId);
                throw new BadRequestException("Cannot delete category because it has subcategories.");
            }

            if (category.Products.Any())
            {
                _logger.LogWarning("Cannot delete category {CategoryId} because it has products", categoryId);
                throw new BadRequestException("Cannot delete category because it has products.");
            }

            _categoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} deleted", categoryId);
        }

        public async Task<CategoryDto> GetByIdAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                _logger.LogWarning("Cannot get category with invalid id {CategoryId}", categoryId);
                throw new BadRequestException("CategoryId must be greater than 0.");
            }

            var category = await _categoryRepository.GetByIdWithChildrenAsync(categoryId);

            if (category is null)
            {
                _logger.LogWarning("Category {CategoryId} was not found", categoryId);
                throw new NotFoundException("Category not found.");
            }

            return category.MapToDto();
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.MapToDtoList();
        }

        public async Task<IReadOnlyList<CategoryDto>> GetTreeAsync()
        {
            var categories = await _categoryRepository.GetAllWithChildrenAsync();

            return categories.MapToDtoList();
        }


    }
}
