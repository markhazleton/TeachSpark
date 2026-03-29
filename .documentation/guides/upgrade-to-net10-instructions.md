# .NET 10.0 Upgrade Instructions

## Summary

The project has been successfully upgraded to .NET 10.0, but Visual Studio may be caching the old .NET 9.0 target framework information.

## Current Status

- ? `.csproj` file updated to `<TargetFramework>net10.0</TargetFramework>`
- ? All NuGet packages updated to the latest .NET 10-compatible versions
- ? .NET 10.0 SDK (10.0.101) is installed
- ? `global.json` configured for SDK 10.0.101
- ? Publish profile updated to `net10.0`
- ??  Visual Studio may need to reload the project

## Solution

### Option 1: Reload Project in Visual Studio (Recommended)

1. In Visual Studio, right-click on the `TeachSpark.Web` project in Solution Explorer
2. Select **"Unload Project"**
3. Right-click again and select **"Reload Project"**
4. Clean and rebuild the solution

### Option 2: Close and Reopen Solution

1. Close Visual Studio completely
2. Reopen the solution
3. Visual Studio will detect the new target framework
4. Clean and rebuild

### Option 3: Command Line Build (Workaround)

If Visual Studio continues to have issues, you can build from the command line:

```powershell
# Clean the solution
dotnet clean TeachSpark.Web\TeachSpark.Web.csproj

# Remove obj and bin folders
Remove-Item -Path "TeachSpark.Web\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "TeachSpark.Web\bin" -Recurse -Force -ErrorAction SilentlyContinue

# Restore with explicit target framework
dotnet restore TeachSpark.Web\TeachSpark.Web.csproj /p:TargetFramework=net10.0

# Build with explicit target framework
dotnet build TeachSpark.Web\TeachSpark.Web.csproj /p:TargetFramework=net10.0
```

### Option 4: Clear All Caches

```powershell
# Clear Visual Studio cache
Remove-Item -Path ".vs" -Recurse -Force -ErrorAction SilentlyContinue

# Clear NuGet caches
dotnet nuget locals all --clear

# Clean project
dotnet clean

# Rebuild
dotnet build /p:TargetFramework=net10.0
```

## Verification

After reloading, verify the correct framework is being used:

```powershell
# Check SDK version
dotnet --version
# Should output: 10.0.101

# Check project target framework
dotnet msbuild TeachSpark.Web\TeachSpark.Web.csproj /t:_GetTargetFrameworksOutput

# Build and verify
dotnet build TeachSpark.Web\TeachSpark.Web.csproj
```

## Updated Packages

The following packages were updated to current versions compatible with .NET 10:

- Markdig: 0.44.0
- Microsoft.AspNetCore.Identity.EntityFrameworkCore: 10.0.1
- Microsoft.AspNetCore.Identity.UI: 10.0.1
- Microsoft.EntityFrameworkCore.Sqlite: 10.0.1
- Microsoft.EntityFrameworkCore.Tools: 10.0.1
- Microsoft.Extensions.AI: 10.1.1
- Microsoft.VisualStudio.Web.CodeGeneration.Design: 10.0.1
- OpenAI: 2.8.0
- Serilog.Extensions.Logging: 10.0.0
- Serilog.Sinks.Console: 6.1.1

## Known Issues

### Visual Studio Caching

Visual Studio may cache the target framework information and continue to report the project as .NET 9.0 even after the `.csproj` file has been updated. This is why reloading the project or restarting Visual Studio is necessary.

### Build from IDE vs Command Line

- **Command line builds** with `/p:TargetFramework=net10.0` work correctly
- **Visual Studio builds** may fail until the project is reloaded

## Additional Notes

- The project is configured with `DisableScopedCss` and uses webpack for asset management
- NPM build tasks are integrated into the MSBuild process
- All Serilog and third-party packages are compatible with .NET 10.0
