using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Mapping;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    public class WorksheetTemplateService : BaseCrudService<WorksheetTemplate, WorksheetTemplateViewModel, CreateWorksheetTemplateViewModel, UpdateWorksheetTemplateViewModel>, IWorksheetTemplateService
    {
        public WorksheetTemplateService(ApplicationDbContext context) : base(context)
        {
        }

        protected override IQueryable<WorksheetTemplate> GetBaseQuery()
        {
            return _dbSet.Include(t => t.User);
        }

        protected override WorksheetTemplateViewModel MapToViewModel(WorksheetTemplate entity)
        {
            return entity.ToViewModel();
        }

        protected override WorksheetTemplate MapToEntity(CreateWorksheetTemplateViewModel createModel, string? userId)
        {
            return createModel.ToEntity(userId);
        }

        protected override void UpdateEntity(UpdateWorksheetTemplateViewModel updateModel, WorksheetTemplate entity)
        {
            updateModel.UpdateEntity(entity);
        }

        protected override int GetEntityId(WorksheetTemplate entity)
        {
            return entity.Id;
        }

        protected override int GetUpdateModelId(UpdateWorksheetTemplateViewModel updateModel)
        {
            return updateModel.Id;
        }

        protected override async Task<WorksheetTemplate?> GetEntityByIdAsync(int id)
        {
            return await GetBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        }

        protected override IQueryable<WorksheetTemplate> ApplySearchFilter(IQueryable<WorksheetTemplate> query, string search)
        {
            return query.Where(t => t.Name.Contains(search) ||
                                   (t.Description != null && t.Description.Contains(search)) ||
                                   t.TemplateType.Contains(search));
        }

        protected override IQueryable<WorksheetTemplate> ApplySorting(IQueryable<WorksheetTemplate> query, string sortBy, bool sortDescending)
        {
            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "templatetype" => sortDescending ? query.OrderByDescending(t => t.TemplateType) : query.OrderBy(t => t.TemplateType),
                "createdat" => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "updatedat" => sortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
                "ispublic" => sortDescending ? query.OrderByDescending(t => t.IsPublic) : query.OrderBy(t => t.IsPublic),
                "issystem" => sortDescending ? query.OrderByDescending(t => t.IsSystem) : query.OrderBy(t => t.IsSystem),
                _ => ApplyDefaultSorting(query)
            };
        }

        protected override IQueryable<WorksheetTemplate> ApplyDefaultSorting(IQueryable<WorksheetTemplate> query)
        {
            return query.OrderByDescending(t => t.IsSystem).ThenBy(t => t.Name);
        }

        protected override ServiceResult<bool> ValidateCreate(CreateWorksheetTemplateViewModel createModel, string? userId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(createModel.Name))
                errors.Add("Name is required");

            if (string.IsNullOrWhiteSpace(createModel.TemplateType))
                errors.Add("Template type is required");

            if (string.IsNullOrWhiteSpace(createModel.LayoutJson))
                errors.Add("Layout JSON is required");

            if (errors.Any())
                return ServiceResult<bool>.ValidationErrorResult(errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        protected override ServiceResult<bool> ValidateUpdate(UpdateWorksheetTemplateViewModel updateModel)
        {
            var errors = new List<string>();

            if (updateModel.Id <= 0)
                errors.Add("Valid ID is required");

            if (string.IsNullOrWhiteSpace(updateModel.Name))
                errors.Add("Name is required");

            if (string.IsNullOrWhiteSpace(updateModel.TemplateType))
                errors.Add("Template type is required");

            if (string.IsNullOrWhiteSpace(updateModel.LayoutJson))
                errors.Add("Layout JSON is required");

            if (errors.Any())
                return ServiceResult<bool>.ValidationErrorResult(errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        // WorksheetTemplate-specific methods
        public async Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetPublicTemplatesAsync(QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(t => t.IsPublic);

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

                var result = new PagedResult<WorksheetTemplateViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.ErrorResult($"Error retrieving public templates: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetByUserIdAsync(string userId, QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(t => t.UserId == userId);

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

                var result = new PagedResult<WorksheetTemplateViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.ErrorResult($"Error retrieving user templates: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetByTemplateTypeAsync(string templateType, QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(t => t.TemplateType == templateType);

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

                var result = new PagedResult<WorksheetTemplateViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.ErrorResult($"Error retrieving templates by type: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetSystemTemplatesAsync(QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(t => t.IsSystem);

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

                var result = new PagedResult<WorksheetTemplateViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<WorksheetTemplateViewModel>>.ErrorResult($"Error retrieving system templates: {ex.Message}");
            }
        }
    }
}
