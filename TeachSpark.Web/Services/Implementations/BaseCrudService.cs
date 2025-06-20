using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TeachSpark.Web.Data;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    public abstract class BaseCrudService<TEntity, TViewModel, TCreateViewModel, TUpdateViewModel> : ICrudService<TViewModel, TCreateViewModel, TUpdateViewModel>
        where TEntity : class
        where TViewModel : class
        where TCreateViewModel : class
        where TUpdateViewModel : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        protected BaseCrudService(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<ServiceResult<PagedResult<TViewModel>>> GetAllAsync(QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = ApplySearchFilter(query, parameters.Search);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
                }
                else
                {
                    query = ApplyDefaultSorting(query);
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var viewModels = items.Select(MapToViewModel).ToList();

                var result = new PagedResult<TViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<TViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<TViewModel>>.ErrorResult($"Error retrieving data: {ex.Message}");
            }
        }

        public virtual async Task<ServiceResult<TViewModel?>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await GetEntityByIdAsync(id);
                if (entity == null)
                {
                    return ServiceResult<TViewModel?>.SuccessResult(null);
                }

                var viewModel = MapToViewModel(entity);
                return ServiceResult<TViewModel?>.SuccessResult(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<TViewModel?>.ErrorResult($"Error retrieving item with ID {id}: {ex.Message}");
            }
        }

        public virtual async Task<ServiceResult<TViewModel>> CreateAsync(TCreateViewModel createModel, string? userId = null)
        {
            try
            {
                var validationResult = ValidateCreate(createModel, userId);
                if (!validationResult.Success)
                {
                    return ServiceResult<TViewModel>.ValidationErrorResult(validationResult.ValidationErrors);
                }

                var entity = MapToEntity(createModel, userId);
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();

                // Reload entity with related data
                var createdEntity = await GetEntityByIdAsync(GetEntityId(entity));
                var viewModel = MapToViewModel(createdEntity!);

                return ServiceResult<TViewModel>.SuccessResult(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<TViewModel>.ErrorResult($"Error creating item: {ex.Message}");
            }
        }

        public virtual async Task<ServiceResult<TViewModel>> UpdateAsync(TUpdateViewModel updateModel)
        {
            try
            {
                var validationResult = ValidateUpdate(updateModel);
                if (!validationResult.Success)
                {
                    return ServiceResult<TViewModel>.ValidationErrorResult(validationResult.ValidationErrors);
                }

                var entityId = GetUpdateModelId(updateModel);
                var entity = await _dbSet.FindAsync(entityId);

                if (entity == null)
                {
                    return ServiceResult<TViewModel>.ErrorResult($"Item with ID {entityId} not found");
                }

                UpdateEntity(updateModel, entity);
                await _context.SaveChangesAsync();

                // Reload entity with related data
                var updatedEntity = await GetEntityByIdAsync(entityId);
                var viewModel = MapToViewModel(updatedEntity!);

                return ServiceResult<TViewModel>.SuccessResult(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<TViewModel>.ErrorResult($"Error updating item: {ex.Message}");
            }
        }

        public virtual async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    return ServiceResult<bool>.ErrorResult($"Item with ID {id} not found");
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.ErrorResult($"Error deleting item with ID {id}: {ex.Message}");
            }
        }

        public virtual async Task<ServiceResult<bool>> ExistsAsync(int id)
        {
            try
            {
                var exists = await _dbSet.FindAsync(id) != null;
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.ErrorResult($"Error checking if item exists: {ex.Message}");
            }
        }

        // Abstract methods to be implemented by derived classes
        protected abstract IQueryable<TEntity> GetBaseQuery();
        protected abstract TViewModel MapToViewModel(TEntity entity);
        protected abstract TEntity MapToEntity(TCreateViewModel createModel, string? userId);
        protected abstract void UpdateEntity(TUpdateViewModel updateModel, TEntity entity);
        protected abstract int GetEntityId(TEntity entity);
        protected abstract int GetUpdateModelId(TUpdateViewModel updateModel);
        protected abstract Task<TEntity?> GetEntityByIdAsync(int id);

        // Virtual methods that can be overridden by derived classes
        protected virtual IQueryable<TEntity> ApplySearchFilter(IQueryable<TEntity> query, string search)
        {
            return query; // Default implementation does nothing
        }

        protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sortBy, bool sortDescending)
        {
            return query; // Default implementation does nothing
        }

        protected virtual IQueryable<TEntity> ApplyDefaultSorting(IQueryable<TEntity> query)
        {
            return query; // Default implementation does nothing
        }

        protected virtual ServiceResult<bool> ValidateCreate(TCreateViewModel createModel, string? userId)
        {
            return ServiceResult<bool>.SuccessResult(true); // Default implementation passes validation
        }

        protected virtual ServiceResult<bool> ValidateUpdate(TUpdateViewModel updateModel)
        {
            return ServiceResult<bool>.SuccessResult(true); // Default implementation passes validation
        }
    }
}
