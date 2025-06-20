using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Mapping;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    public class WorksheetService : BaseCrudService<Worksheet, WorksheetViewModel, CreateWorksheetViewModel, UpdateWorksheetViewModel>, IWorksheetService
    {
        public WorksheetService(ApplicationDbContext context) : base(context)
        {
        }

        protected override IQueryable<Worksheet> GetBaseQuery()
        {
            return _dbSet.Include(w => w.User)
                         .Include(w => w.CommonCoreStandard)
                         .Include(w => w.AcademicStandard)
                         .Include(w => w.BloomLevel)
                         .Include(w => w.Template);
        }

        protected override WorksheetViewModel MapToViewModel(Worksheet entity)
        {
            return entity.ToViewModel();
        }

        protected override Worksheet MapToEntity(CreateWorksheetViewModel createModel, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId is required for worksheet creation");

            return createModel.ToEntity(userId);
        }

        protected override void UpdateEntity(UpdateWorksheetViewModel updateModel, Worksheet entity)
        {
            updateModel.UpdateEntity(entity);
        }

        protected override int GetEntityId(Worksheet entity)
        {
            return entity.Id;
        }

        protected override int GetUpdateModelId(UpdateWorksheetViewModel updateModel)
        {
            return updateModel.Id;
        }

        protected override async Task<Worksheet?> GetEntityByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(w => w.Id == id);
        }

        protected override IQueryable<Worksheet> ApplySearchFilter(IQueryable<Worksheet> query, string search)
        {
            return query.Where(w => w.Title.Contains(search) ||
                                   (w.Description != null && w.Description.Contains(search)) ||
                                   (w.Tags != null && w.Tags.Contains(search)) ||
                                   w.WorksheetType.Contains(search));
        }

        protected override IQueryable<Worksheet> ApplySorting(IQueryable<Worksheet> query, string sortBy, bool sortDescending)
        {
            return sortBy.ToLower() switch
            {
                "title" => sortDescending ? query.OrderByDescending(w => w.Title) : query.OrderBy(w => w.Title),
                "createdat" => sortDescending ? query.OrderByDescending(w => w.CreatedAt) : query.OrderBy(w => w.CreatedAt),
                "updatedat" => sortDescending ? query.OrderByDescending(w => w.UpdatedAt) : query.OrderBy(w => w.UpdatedAt),
                "worksheettype" => sortDescending ? query.OrderByDescending(w => w.WorksheetType) : query.OrderBy(w => w.WorksheetType),
                _ => ApplyDefaultSorting(query)
            };
        }

        protected override IQueryable<Worksheet> ApplyDefaultSorting(IQueryable<Worksheet> query)
        {
            return query.OrderByDescending(w => w.UpdatedAt);
        }

        protected override ServiceResult<bool> ValidateCreate(CreateWorksheetViewModel createModel, string? userId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(userId))
                errors.Add("User ID is required");

            if (string.IsNullOrWhiteSpace(createModel.Title))
                errors.Add("Title is required");

            if (string.IsNullOrWhiteSpace(createModel.ContentJson))
                errors.Add("Content is required");

            if (errors.Any())
                return ServiceResult<bool>.ValidationErrorResult(errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        protected override ServiceResult<bool> ValidateUpdate(UpdateWorksheetViewModel updateModel)
        {
            var errors = new List<string>();

            if (updateModel.Id <= 0)
                errors.Add("Valid ID is required");

            if (string.IsNullOrWhiteSpace(updateModel.Title))
                errors.Add("Title is required");

            if (string.IsNullOrWhiteSpace(updateModel.ContentJson))
                errors.Add("Content is required");

            if (errors.Any())
                return ServiceResult<bool>.ValidationErrorResult(errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        // Worksheet-specific methods
        public async Task<ServiceResult<PagedResult<WorksheetViewModel>>> GetByUserIdAsync(string userId, QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(w => w.UserId == userId);

                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = ApplySearchFilter(query, parameters.Search);
                }

                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
                }
                else
                {
                    query = ApplyDefaultSorting(query);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var viewModels = items.Select(MapToViewModel).ToList();

                var result = new PagedResult<WorksheetViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetViewModel>>.ErrorResult($"Error retrieving user worksheets: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<WorksheetViewModel>>> GetPublicWorksheetsAsync(QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(w => w.IsPublic);

                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = ApplySearchFilter(query, parameters.Search);
                }

                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
                }
                else
                {
                    query = ApplyDefaultSorting(query);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var viewModels = items.Select(MapToViewModel).ToList();

                var result = new PagedResult<WorksheetViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetViewModel>>.ErrorResult($"Error retrieving public worksheets: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<WorksheetViewModel>>> GetFavoritesByUserIdAsync(string userId, QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(w => w.UserId == userId && w.IsFavorite);

                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = ApplySearchFilter(query, parameters.Search);
                }

                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
                }
                else
                {
                    query = ApplyDefaultSorting(query);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var viewModels = items.Select(MapToViewModel).ToList();

                var result = new PagedResult<WorksheetViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetViewModel>>.ErrorResult($"Error retrieving favorite worksheets: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ToggleFavoriteAsync(int worksheetId, string userId)
        {
            try
            {
                var worksheet = await _dbSet.FirstOrDefaultAsync(w => w.Id == worksheetId && w.UserId == userId);
                if (worksheet == null)
                {
                    return ServiceResult<bool>.ErrorResult("Worksheet not found or access denied");
                }

                worksheet.IsFavorite = !worksheet.IsFavorite;
                worksheet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.SuccessResult(worksheet.IsFavorite);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.ErrorResult($"Error toggling favorite status: {ex.Message}");
            }
        }
    }
}
