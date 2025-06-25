using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Mapping
{
    public static class WorksheetTemplateMappingExtensions
    {
        public static WorksheetTemplateViewModel ToViewModel(this WorksheetTemplate entity)
        {
            return new WorksheetTemplateViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                TemplateType = entity.TemplateType,
                PreviewImageUrl = entity.PreviewImageUrl,
                IsPublic = entity.IsPublic,
                IsSystem = entity.IsSystem,
                UserId = entity.UserId,
                UserDisplayName = entity.User != null ? $"{entity.User.FirstName} {entity.User.LastName}" : null,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static WorksheetTemplate ToEntity(this CreateWorksheetTemplateViewModel viewModel, string? userId = null)
        {
            return new WorksheetTemplate
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                TemplateType = viewModel.TemplateType,
                PreviewImageUrl = viewModel.PreviewImageUrl,
                IsPublic = viewModel.IsPublic,
                IsSystem = viewModel.IsSystem,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateEntity(this UpdateWorksheetTemplateViewModel viewModel, WorksheetTemplate entity)
        {
            entity.Name = viewModel.Name;
            entity.Description = viewModel.Description;
            entity.TemplateType = viewModel.TemplateType;
            entity.PreviewImageUrl = viewModel.PreviewImageUrl;
            entity.IsPublic = viewModel.IsPublic;
            entity.IsSystem = viewModel.IsSystem;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
