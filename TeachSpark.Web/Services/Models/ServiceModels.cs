namespace TeachSpark.Web.Services.Models
{
    public class QueryParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public bool SortDescending { get; set; } = false;
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public static ServiceResult<T> SuccessResult(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResult<T> ErrorResult(string errorMessage)
        {
            return new ServiceResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static ServiceResult<T> ValidationErrorResult(List<string> validationErrors)
        {
            return new ServiceResult<T>
            {
                Success = false,
                ValidationErrors = validationErrors
            };
        }
    }
}
