using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Interfaces
{
    public interface ICrudService<TViewModel, TCreateViewModel, TUpdateViewModel>
        where TViewModel : class
        where TCreateViewModel : class
        where TUpdateViewModel : class
    {
        Task<ServiceResult<PagedResult<TViewModel>>> GetAllAsync(QueryParameters parameters);
        Task<ServiceResult<TViewModel?>> GetByIdAsync(int id);
        Task<ServiceResult<TViewModel>> CreateAsync(TCreateViewModel createModel, string? userId = null);
        Task<ServiceResult<TViewModel>> UpdateAsync(TUpdateViewModel updateModel);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> ExistsAsync(int id);
    }

    public interface IWorksheetService : ICrudService<WorksheetViewModel, CreateWorksheetViewModel, UpdateWorksheetViewModel>
    {
        Task<ServiceResult<PagedResult<WorksheetViewModel>>> GetByUserIdAsync(string userId, QueryParameters parameters);
        Task<ServiceResult<PagedResult<WorksheetViewModel>>> GetPublicWorksheetsAsync(QueryParameters parameters);
        Task<ServiceResult<PagedResult<WorksheetViewModel>>> GetFavoritesByUserIdAsync(string userId, QueryParameters parameters);
        Task<ServiceResult<bool>> ToggleFavoriteAsync(int worksheetId, string userId);
    }

    public interface ICommonCoreStandardService : ICrudService<CommonCoreStandardViewModel, CreateCommonCoreStandardViewModel, UpdateCommonCoreStandardViewModel>
    {
        Task<ServiceResult<PagedResult<CommonCoreStandardViewModel>>> GetByGradeAsync(string grade, QueryParameters parameters);
        Task<ServiceResult<PagedResult<CommonCoreStandardViewModel>>> GetBySubjectAsync(string subject, QueryParameters parameters);
        Task<ServiceResult<CommonCoreStandardViewModel?>> GetByCodeAsync(string code);
    }

    public interface IWorksheetTemplateService : ICrudService<WorksheetTemplateViewModel, CreateWorksheetTemplateViewModel, UpdateWorksheetTemplateViewModel>
    {
        Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetPublicTemplatesAsync(QueryParameters parameters);
        Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetByUserIdAsync(string userId, QueryParameters parameters);
        Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetByTemplateTypeAsync(string templateType, QueryParameters parameters);
        Task<ServiceResult<PagedResult<WorksheetTemplateViewModel>>> GetSystemTemplatesAsync(QueryParameters parameters);
    }
}
