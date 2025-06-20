using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Mapping;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    public class CommonCoreStandardService : BaseCrudService<CommonCoreStandard, CommonCoreStandardViewModel, CreateCommonCoreStandardViewModel, UpdateCommonCoreStandardViewModel>, ICommonCoreStandardService
    {
        public CommonCoreStandardService(ApplicationDbContext context) : base(context)
        {
        }

        protected override IQueryable<CommonCoreStandard> GetBaseQuery()
        {
            return _dbSet;
        }

        protected override CommonCoreStandardViewModel MapToViewModel(CommonCoreStandard entity)
        {
            return entity.ToViewModel();
        }

        protected override CommonCoreStandard MapToEntity(CreateCommonCoreStandardViewModel createModel, string? userId)
        {
            return createModel.ToEntity();
        }

        protected override void UpdateEntity(UpdateCommonCoreStandardViewModel updateModel, CommonCoreStandard entity)
        {
            updateModel.UpdateEntity(entity);
        }

        protected override int GetEntityId(CommonCoreStandard entity)
        {
            return entity.Id;
        }

        protected override int GetUpdateModelId(UpdateCommonCoreStandardViewModel updateModel)
        {
            return updateModel.Id;
        }

        protected override async Task<CommonCoreStandard?> GetEntityByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        protected override IQueryable<CommonCoreStandard> ApplySearchFilter(IQueryable<CommonCoreStandard> query, string search)
        {
            return query.Where(c => c.Code.Contains(search) ||
                                   c.Description.Contains(search) ||
                                   c.Subject.Contains(search) ||
                                   c.Domain.Contains(search) ||
                                   (c.Category != null && c.Category.Contains(search)));
        }

        protected override IQueryable<CommonCoreStandard> ApplySorting(IQueryable<CommonCoreStandard> query, string sortBy, bool sortDescending)
        {
            return sortBy.ToLower() switch
            {
                "code" => sortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
                "grade" => sortDescending ? query.OrderByDescending(c => c.Grade) : query.OrderBy(c => c.Grade),
                "subject" => sortDescending ? query.OrderByDescending(c => c.Subject) : query.OrderBy(c => c.Subject),
                "domain" => sortDescending ? query.OrderByDescending(c => c.Domain) : query.OrderBy(c => c.Domain),
                "sortorder" => sortDescending ? query.OrderByDescending(c => c.SortOrder) : query.OrderBy(c => c.SortOrder),
                _ => ApplyDefaultSorting(query)
            };
        }

        protected override IQueryable<CommonCoreStandard> ApplyDefaultSorting(IQueryable<CommonCoreStandard> query)
        {
            return query.OrderBy(c => c.Grade).ThenBy(c => c.Subject).ThenBy(c => c.Code);
        }

        protected override ServiceResult<bool> ValidateCreate(CreateCommonCoreStandardViewModel createModel, string? userId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(createModel.Code))
                errors.Add("Code is required");

            if (string.IsNullOrWhiteSpace(createModel.Grade))
                errors.Add("Grade is required");

            if (string.IsNullOrWhiteSpace(createModel.Subject))
                errors.Add("Subject is required");

            if (string.IsNullOrWhiteSpace(createModel.Domain))
                errors.Add("Domain is required");

            if (string.IsNullOrWhiteSpace(createModel.Description))
                errors.Add("Description is required");

            if (errors.Any())
                return ServiceResult<bool>.ValidationErrorResult(errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        protected override ServiceResult<bool> ValidateUpdate(UpdateCommonCoreStandardViewModel updateModel)
        {
            var errors = new List<string>();

            if (updateModel.Id <= 0)
                errors.Add("Valid ID is required");

            if (string.IsNullOrWhiteSpace(updateModel.Code))
                errors.Add("Code is required");

            if (string.IsNullOrWhiteSpace(updateModel.Grade))
                errors.Add("Grade is required");

            if (string.IsNullOrWhiteSpace(updateModel.Subject))
                errors.Add("Subject is required");

            if (string.IsNullOrWhiteSpace(updateModel.Domain))
                errors.Add("Domain is required");

            if (string.IsNullOrWhiteSpace(updateModel.Description))
                errors.Add("Description is required");

            if (errors.Any())
                return ServiceResult<bool>.ValidationErrorResult(errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        // CommonCoreStandard-specific methods
        public async Task<ServiceResult<PagedResult<CommonCoreStandardViewModel>>> GetByGradeAsync(string grade, QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(c => c.Grade == grade);

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

                var result = new PagedResult<CommonCoreStandardViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<CommonCoreStandardViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<CommonCoreStandardViewModel>>.ErrorResult($"Error retrieving standards by grade: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<CommonCoreStandardViewModel>>> GetBySubjectAsync(string subject, QueryParameters parameters)
        {
            try
            {
                var query = GetBaseQuery().Where(c => c.Subject == subject);

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

                var result = new PagedResult<CommonCoreStandardViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize
                };

                return ServiceResult<PagedResult<CommonCoreStandardViewModel>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<CommonCoreStandardViewModel>>.ErrorResult($"Error retrieving standards by subject: {ex.Message}");
            }
        }

        public async Task<ServiceResult<CommonCoreStandardViewModel?>> GetByCodeAsync(string code)
        {
            try
            {
                var entity = await _dbSet.FirstOrDefaultAsync(c => c.Code == code);
                if (entity == null)
                {
                    return ServiceResult<CommonCoreStandardViewModel?>.SuccessResult(null);
                }

                var viewModel = MapToViewModel(entity);
                return ServiceResult<CommonCoreStandardViewModel?>.SuccessResult(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<CommonCoreStandardViewModel?>.ErrorResult($"Error retrieving standard by code: {ex.Message}");
            }
        }
    }
}
