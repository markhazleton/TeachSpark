using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Mapping
{
    public static class WorksheetMappingExtensions
    {
        public static WorksheetViewModel ToViewModel(this Worksheet entity)
        {
            return new WorksheetViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                UserId = entity.UserId,
                UserDisplayName = entity.User != null ? $"{entity.User.FirstName} {entity.User.LastName}" : null,
                CommonCoreStandardId = entity.CommonCoreStandardId,
                CommonCoreStandardCode = entity.CommonCoreStandard?.Code,
                CommonCoreStandardDescription = entity.CommonCoreStandard?.Description,
                AcademicStandardId = entity.AcademicStandardId,
                AcademicStandardCode = entity.AcademicStandard?.GleCode,
                AcademicStandardDescription = entity.AcademicStandard?.Statement,
                BloomLevelId = entity.BloomLevelId,
                BloomLevelName = entity.BloomLevel?.Name,
                TemplateId = entity.TemplateId,
                TemplateName = entity.Template?.Name,
                ContentJson = entity.ContentJson,
                SourceText = entity.SourceText,
                WorksheetType = entity.WorksheetType,
                DifficultyLevel = entity.DifficultyLevel,
                AccessibilityOptions = entity.AccessibilityOptions,
                Tags = entity.Tags,
                IsPublic = entity.IsPublic,
                IsFavorite = entity.IsFavorite,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static Worksheet ToEntity(this CreateWorksheetViewModel viewModel, string userId)
        {
            return new Worksheet
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                UserId = userId,
                CommonCoreStandardId = viewModel.CommonCoreStandardId,
                AcademicStandardId = viewModel.AcademicStandardId,
                BloomLevelId = viewModel.BloomLevelId,
                TemplateId = viewModel.TemplateId,
                ContentJson = viewModel.ContentJson,
                SourceText = viewModel.SourceText,
                WorksheetType = viewModel.WorksheetType,
                DifficultyLevel = viewModel.DifficultyLevel,
                AccessibilityOptions = viewModel.AccessibilityOptions,
                Tags = viewModel.Tags,
                IsPublic = viewModel.IsPublic,
                IsFavorite = viewModel.IsFavorite,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateEntity(this UpdateWorksheetViewModel viewModel, Worksheet entity)
        {
            entity.Title = viewModel.Title;
            entity.Description = viewModel.Description;
            entity.CommonCoreStandardId = viewModel.CommonCoreStandardId;
            entity.AcademicStandardId = viewModel.AcademicStandardId;
            entity.BloomLevelId = viewModel.BloomLevelId;
            entity.TemplateId = viewModel.TemplateId;
            entity.ContentJson = viewModel.ContentJson;
            entity.SourceText = viewModel.SourceText;
            entity.WorksheetType = viewModel.WorksheetType;
            entity.DifficultyLevel = viewModel.DifficultyLevel;
            entity.AccessibilityOptions = viewModel.AccessibilityOptions;
            entity.Tags = viewModel.Tags;
            entity.IsPublic = viewModel.IsPublic;
            entity.IsFavorite = viewModel.IsFavorite;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
