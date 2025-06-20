using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Mapping
{
    public static class CommonCoreStandardMappingExtensions
    {
        public static CommonCoreStandardViewModel ToViewModel(this CommonCoreStandard entity)
        {
            return new CommonCoreStandardViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Grade = entity.Grade,
                Subject = entity.Subject,
                Domain = entity.Domain,
                Description = entity.Description,
                Category = entity.Category,
                ExampleActivities = entity.ExampleActivities,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder
            };
        }

        public static CommonCoreStandard ToEntity(this CreateCommonCoreStandardViewModel viewModel)
        {
            return new CommonCoreStandard
            {
                Code = viewModel.Code,
                Grade = viewModel.Grade,
                Subject = viewModel.Subject,
                Domain = viewModel.Domain,
                Description = viewModel.Description,
                Category = viewModel.Category,
                ExampleActivities = viewModel.ExampleActivities,
                IsActive = true,
                SortOrder = 0
            };
        }

        public static void UpdateEntity(this UpdateCommonCoreStandardViewModel viewModel, CommonCoreStandard entity)
        {
            entity.Code = viewModel.Code;
            entity.Grade = viewModel.Grade;
            entity.Subject = viewModel.Subject;
            entity.Domain = viewModel.Domain; entity.Description = viewModel.Description;
            entity.Category = viewModel.Category;
            entity.ExampleActivities = viewModel.ExampleActivities;
        }
    }
}
