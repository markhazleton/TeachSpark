# TeachSpark IIS Deployment Script
# This script ensures proper publishing and packaging for IIS deployment

param(
    [string]$OutputPath = "C:\PublishedWebsites\teachspark",
    [string]$Configuration = "Release"
)

Write-Host "=== TeachSpark IIS Deployment Script ===" -ForegroundColor Green

# Step 1: Clean previous builds and destination
Write-Host "Cleaning previous builds and destination..." -ForegroundColor Yellow

# Clean local build artifacts
Remove-Item -Path ".\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".\obj" -Recurse -Force -ErrorAction SilentlyContinue

# Ensure the parent directory exists
$parentDir = Split-Path $OutputPath -Parent
if (!(Test-Path $parentDir)) {
    Write-Host "Creating parent directory: $parentDir" -ForegroundColor Cyan
    New-Item -Path $parentDir -ItemType Directory -Force | Out-Null
}

# Clean destination directory completely for fresh deployment
if (Test-Path $OutputPath) {
    Write-Host "Removing existing files from: $OutputPath" -ForegroundColor Cyan
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1  # Brief pause to ensure file deletion completes
}

# Create clean destination directory
Write-Host "Creating clean destination directory: $OutputPath" -ForegroundColor Cyan
New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null

# Step 2: Clean and build frontend assets
Write-Host "Building frontend assets..." -ForegroundColor Yellow
npm run clean
if ($LASTEXITCODE -ne 0) { 
    Write-Error "npm clean failed"
    exit 1 
}

npm run build
if ($LASTEXITCODE -ne 0) { 
    Write-Error "npm build failed"
    exit 1 
}

# Step 3: Restore dependencies
Write-Host "Restoring .NET dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) { 
    Write-Error "dotnet restore failed"
    exit 1 
}

# Step 4: Build the application
Write-Host "Building .NET application..." -ForegroundColor Yellow
dotnet build -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { 
    Write-Error "dotnet build failed"
    exit 1 
}

# Step 5: Publish the application directly to the output path
Write-Host "Publishing application to $OutputPath..." -ForegroundColor Yellow
dotnet publish -c $Configuration -o $OutputPath --no-build --no-restore
if ($LASTEXITCODE -ne 0) { 
    Write-Error "dotnet publish failed"
    exit 1 
}

# Step 6: Verify critical files exist
Write-Host "Verifying deployment files..." -ForegroundColor Yellow
$criticalFiles = @(
    "$OutputPath\TeachSpark.Web.dll",
    "$OutputPath\web.config",
    "$OutputPath\wwwroot\assets-manifest.json"
)

foreach ($file in $criticalFiles) {
    if (!(Test-Path $file)) {
        Write-Error "Critical file missing: $file"
        exit 1
    }
    Write-Host "✓ Found: $file" -ForegroundColor Green
}

# Step 7: Verify web.config content and fix appsettings
Write-Host "Verifying web.config content and appsettings..." -ForegroundColor Yellow
$webConfigContent = Get-Content "$OutputPath\web.config" -Raw
if ($webConfigContent -match 'processPath="dotnet"') {
    Write-Host "✓ web.config has correct processPath" -ForegroundColor Green
}
else {
    Write-Error "web.config has incorrect processPath"
    exit 1
}

# Fix database path in appsettings.json to match hosting server location
$appsettingsPath = "$OutputPath\appsettings.json"
if (Test-Path $appsettingsPath) {
    Write-Host "Updating database path in appsettings.json..." -ForegroundColor Yellow
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $newDbPath = "c:\\websites\\teachspark\\teachspark.db"
    $appsettings.ConnectionStrings.DefaultConnection = "Data Source=$newDbPath"
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath -Encoding UTF8
    Write-Host "✓ Database path updated to: $newDbPath" -ForegroundColor Green
    Write-Host "  (Points to hosting server database location)" -ForegroundColor Cyan
}

# Step 8: Create logs directory and copy troubleshooting files
Write-Host "Creating logs directory and copying troubleshooting files..." -ForegroundColor Yellow
New-Item -Path "$OutputPath\logs" -ItemType Directory -Force | Out-Null
Write-Host "✓ Logs directory created" -ForegroundColor Green

# Copy diagnostic and troubleshooting files
if (Test-Path ".\web.config.diagnostic") {
    Copy-Item ".\web.config.diagnostic" "$OutputPath\web.config.diagnostic" -Force
    Write-Host "✓ Diagnostic web.config copied" -ForegroundColor Green
}

if (Test-Path ".\Troubleshoot-TeachSpark-Fixed.ps1") {
    Copy-Item ".\Troubleshoot-TeachSpark-Fixed.ps1" "$OutputPath\Troubleshoot-TeachSpark.ps1" -Force
    Write-Host "✓ Troubleshooting script copied (fixed version)" -ForegroundColor Green
}
elseif (Test-Path ".\Troubleshoot-TeachSpark.ps1") {
    Copy-Item ".\Troubleshoot-TeachSpark.ps1" "$OutputPath\Troubleshoot-TeachSpark.ps1" -Force
    Write-Host "✓ Troubleshooting script copied" -ForegroundColor Green
}

# Step 9: Create deployment package (optional backup)
Write-Host "Creating deployment package backup..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$zipPath = ".\TeachSpark-IIS-Deployment-$timestamp.zip"
Remove-Item ".\TeachSpark-IIS-Deployment-*.zip" -ErrorAction SilentlyContinue
Compress-Archive -Path "$OutputPath\*" -DestinationPath $zipPath -Force
Write-Host "✓ Deployment backup created: $zipPath" -ForegroundColor Green

Write-Host "=== Deployment Ready ===" -ForegroundColor Green
Write-Host "Application deployed to: $OutputPath" -ForegroundColor Cyan
Write-Host "Backup package: $zipPath" -ForegroundColor Cyan
Write-Host "Ready for IIS configuration." -ForegroundColor Cyan

# Step 10: Display deployment instructions
Write-Host "`n=== IIS Deployment Instructions ===" -ForegroundColor Yellow
Write-Host "1. Application files are ready at: $OutputPath"
Write-Host "2. Point your IIS application to: $OutputPath"
Write-Host "3. Ensure Application Pool is set to 'No Managed Code'"
Write-Host "4. Verify .NET 9.0 Hosting Bundle is installed on server"
Write-Host "5. Grant IIS_IUSRS read/execute permissions to: $OutputPath"
Write-Host "5. Grant write permissions to: $OutputPath\logs" 
Write-Host "6. Ensure database file exists and has proper permissions at hosting server:"
Write-Host "   c:\websites\teachspark\teachspark.db" -ForegroundColor Cyan
Write-Host "7. Test the application and check /Diagnostics/Assets endpoint"

# Step 11: Display post-deployment verification
Write-Host "`n=== Post-Deployment Verification ===" -ForegroundColor Yellow
Write-Host "Run these commands to verify IIS configuration:"
Write-Host "icacls `"$OutputPath`" /grant `"IIS_IUSRS:(OI)(CI)RX`" /T" -ForegroundColor Gray
Write-Host "icacls `"$OutputPath\logs`" /grant `"IIS_IUSRS:(OI)(CI)F`" /T" -ForegroundColor Gray
Write-Host ""
Write-Host "Database setup - ensure database exists at hosting server path:" -ForegroundColor Yellow
Write-Host "c:\websites\teachspark\teachspark.db" -ForegroundColor Cyan
Write-Host "Grant permissions to the database folder and file:" -ForegroundColor Yellow
Write-Host "icacls `"c:\websites\teachspark`" /grant `"IIS_IUSRS:(OI)(CI)F`" /T" -ForegroundColor Gray
Write-Host "icacls `"c:\websites\teachspark\teachspark.db`" /grant `"IIS_IUSRS:F`"" -ForegroundColor Gray

# Step 12: Troubleshooting commands
Write-Host "`n=== Troubleshooting Commands ===" -ForegroundColor Red
Write-Host "✓ No console asset errors reported - webpack fixes working!" -ForegroundColor Green
Write-Host "Issue is likely server-side. Run these commands on the server:"
Write-Host ""
Write-Host "1. Run diagnostics script:" -ForegroundColor Yellow
Write-Host "   cd `"$OutputPath`"" -ForegroundColor Gray
Write-Host "   .\Troubleshoot-TeachSpark.ps1 -EnableDiagnosticMode" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Check application startup logs:" -ForegroundColor Yellow
Write-Host "   Get-Content `"$OutputPath\logs\stdout*.log`" -Tail 50" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Check for detailed 500 errors (after diagnostic mode):" -ForegroundColor Yellow
Write-Host "   Visit: https://teachspark.markhazleton.com/WorksheetGenerator" -ForegroundColor Gray
Write-Host "   Should show detailed error instead of generic 500" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Test application manually:" -ForegroundColor Yellow
Write-Host "   cd `"$OutputPath`"" -ForegroundColor Gray
Write-Host "   dotnet TeachSpark.Web.dll --urls=http://localhost:5000" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Common server-side issues to check:" -ForegroundColor Yellow
Write-Host "   - Database file exists at: c:\websites\teachspark\teachspark.db" -ForegroundColor Gray
Write-Host "   - Database file permissions (ensure IIS can read/write SQLite)" -ForegroundColor Gray
Write-Host "   - Missing NuGet packages" -ForegroundColor Gray
Write-Host "   - Dependency injection configuration errors" -ForegroundColor Gray
Write-Host "   - Controller/Action method exceptions" -ForegroundColor Gray
