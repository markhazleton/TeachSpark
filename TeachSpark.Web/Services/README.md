# TeachSpark CRUD Services

This document describes the CRUD service layer implementation for the TeachSpark application. The service layer provides abstraction between the controllers and the data layer, exposing view models instead of entities to maintain separation of concerns.

## Architecture Overview

The service layer follows these principles:

1. **Separation of Concerns**: Services only expose view models, never entities
2. **Abstraction**: Controllers interact with services, not directly with DbContext
3. **Reusability**: Common CRUD operations are implemented in a base service
4. **Type Safety**: Strong typing with generics for different entity types
5. **Error Handling**: Consistent error handling with ServiceResult wrapper
6. **Validation**: Input validation at the service layer

## Structure

```
Services/
├── Extensions/
│   └── ServiceCollectionExtensions.cs    # DI registration
├── Implementations/
│   ├── BaseCrudService.cs                # Base CRUD functionality
│   ├── WorksheetService.cs               # Worksheet-specific operations
│   ├── CommonCoreStandardService.cs      # Common Core Standard operations
│   └── WorksheetTemplateService.cs       # Template operations
├── Interfaces/
│   └── IServices.cs                      # Service interfaces
├── Mapping/
│   ├── WorksheetMappingExtensions.cs     # Entity ↔ ViewModel mapping
│   ├── CommonCoreStandardMappingExtensions.cs
│   └── WorksheetTemplateMappingExtensions.cs
└── Models/
    ├── ServiceModels.cs                  # Common models (QueryParameters, ServiceResult, etc.)
    ├── WorksheetViewModel.cs             # Worksheet view models
    ├── CommonCoreStandardViewModel.cs    # Common Core Standard view models
    └── WorksheetTemplateViewModel.cs     # Template view models
```

## Registration

Register the services in your `Program.cs`:

```csharp
using TeachSpark.Web.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCrudServices();
```

## Usage Examples

### Basic CRUD Operations

```csharp
public class WorksheetController : ControllerBase
{
    private readonly IWorksheetService _worksheetService;

    public WorksheetController(IWorksheetService worksheetService)
    {
        _worksheetService = worksheetService;
    }

    // Get all worksheets with pagination
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
    {
        var result = await _worksheetService.GetAllAsync(parameters);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    // Get worksheet by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _worksheetService.GetByIdAsync(id);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        if (result.Data == null)
        {
            return NotFound();
        }
        return Ok(result.Data);
    }

    // Create new worksheet
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorksheetViewModel model)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _worksheetService.CreateAsync(model, userId);
        
        if (!result.Success)
        {
            if (result.ValidationErrors.Any())
            {
                return BadRequest(result.ValidationErrors);
            }
            return BadRequest(result.ErrorMessage);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    // Update worksheet
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorksheetViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _worksheetService.UpdateAsync(model);
        if (!result.Success)
        {
            if (result.ValidationErrors.Any())
            {
                return BadRequest(result.ValidationErrors);
            }
            return BadRequest(result.ErrorMessage);
        }
        
        return Ok(result.Data);
    }

    // Delete worksheet
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _worksheetService.DeleteAsync(id);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        return NoContent();
    }
}
```

### Advanced Queries

```csharp
// Get user's worksheets
var userWorksheets = await _worksheetService.GetByUserIdAsync(userId, parameters);

// Get public worksheets
var publicWorksheets = await _worksheetService.GetPublicWorksheetsAsync(parameters);

// Get user's favorite worksheets
var favorites = await _worksheetService.GetFavoritesByUserIdAsync(userId, parameters);

// Toggle favorite status
var result = await _worksheetService.ToggleFavoriteAsync(worksheetId, userId);

// Get Common Core Standards by grade
var gradeStandards = await _commonCoreService.GetByGradeAsync("5", parameters);

// Get templates by type
var templates = await _templateService.GetByTemplateTypeAsync("reading-comprehension", parameters);
```

## Query Parameters

All services support standardized query parameters:

```csharp
public class QueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10; // Max 100
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
```

## Service Result

All service methods return a `ServiceResult<T>` that provides consistent error handling:

```csharp
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new List<string>();
}
```

## View Models

### Create vs Update vs View Models

- **CreateViewModel**: Used for creating new entities (excludes ID, read-only fields)
- **UpdateViewModel**: Used for updating existing entities (includes ID)
- **ViewModel**: Used for reading/displaying data (includes all fields, related data)

### Mapping Extensions

Mapping between entities and view models is handled by extension methods:

```csharp
// Entity to ViewModel
var viewModel = entity.ToViewModel();

// CreateViewModel to Entity
var entity = createModel.ToEntity(userId);

// UpdateViewModel to Entity (updates existing entity)
updateModel.UpdateEntity(existingEntity);
```

## Best Practices

1. **Always use view models**: Never expose entities directly to controllers
2. **Handle errors gracefully**: Check ServiceResult.Success before using data
3. **Validate input**: Services perform validation and return validation errors
4. **Use pagination**: Always use QueryParameters for list operations
5. **Include related data**: Services automatically include necessary related entities
6. **Separation of concerns**: Services handle business logic, controllers handle HTTP concerns

## Error Handling

Services provide comprehensive error handling:

- **Validation Errors**: Input validation failures
- **Not Found**: Entity doesn't exist
- **Database Errors**: Connection or query issues
- **Business Logic Errors**: Custom validation rules

Example error handling:

```csharp
var result = await service.CreateAsync(model, userId);

if (!result.Success)
{
    if (result.ValidationErrors.Any())
    {
        // Handle validation errors
        return BadRequest(result.ValidationErrors);
    }
    
    // Handle general errors
    return BadRequest(result.ErrorMessage);
}

// Success path
return Ok(result.Data);
```
