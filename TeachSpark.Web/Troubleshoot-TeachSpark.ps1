# TeachSpark Troubleshooting Script
# Run this on the IIS server to diagnose issues

param(
    [string]$AppPath = "C:\PublishedWebsites\teachspark",
    [switch]$EnableDiagnosticMode
)

Write-Host "=== TeachSpark Troubleshooting Script ===" -ForegroundColor Red

# Step 1: Check if application files exist
Write-Host "`n1. Checking application files..." -ForegroundColor Yellow
$criticalFiles = @(
    "$AppPath\TeachSpark.Web.dll",
    "$AppPath\web.config",
    "$AppPath\wwwroot\assets-manifest.json"
)

foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Write-Host "✓ Found: $file" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Missing: $file" -ForegroundColor Red
    }
}

# Step 2: Check web.config content
Write-Host "`n2. Checking web.config configuration..." -ForegroundColor Yellow
if (Test-Path "$AppPath\web.config") {
    $webConfigContent = Get-Content "$AppPath\web.config" -Raw
    
    if ($webConfigContent -match 'processPath="dotnet"') {
        Write-Host "✓ processPath is correct (dotnet)" -ForegroundColor Green
    }
    else {
        Write-Host "✗ processPath is incorrect (should be 'dotnet')" -ForegroundColor Red
    }
    
    if ($webConfigContent -match 'stdoutLogEnabled="true"') {
        Write-Host "✓ stdout logging is enabled" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ stdout logging is disabled" -ForegroundColor Yellow
    }
}
else {
    Write-Host "✗ web.config not found" -ForegroundColor Red
}

# Step 2.5: Check database configuration
Write-Host "`n2.5. Checking database configuration..." -ForegroundColor Yellow
$appsettingsPath = "$AppPath\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $dbConnectionString = $appsettings.ConnectionStrings.DefaultConnection
    Write-Host "Database connection string: $dbConnectionString" -ForegroundColor Cyan
    
    # Extract database file path
    if ($dbConnectionString -match "Data Source=(.+?)(?:;|`$)") {
        $dbPath = $matches[1]
        if (Test-Path $dbPath) {
            Write-Host "✓ Database file exists: $dbPath" -ForegroundColor Green
        }
        else {
            Write-Host "✗ Database file missing: $dbPath" -ForegroundColor Red
            Write-Host "This could cause 500 errors on pages that access the database" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "✗ appsettings.json not found" -ForegroundColor Red
}

# Step 3: Check logs directory and permissions
Write-Host "`n3. Checking logs directory..." -ForegroundColor Yellow
$logsPath = "$AppPath\logs"
if (Test-Path $logsPath) {
    Write-Host "✓ Logs directory exists: $logsPath" -ForegroundColor Green
    
    $logFiles = Get-ChildItem "$logsPath\*.log" -ErrorAction SilentlyContinue
    if ($logFiles) {
        Write-Host "✓ Found $($logFiles.Count) log file(s)" -ForegroundColor Green
        Write-Host "`nMost recent log entries:" -ForegroundColor Cyan
        $latestLog = $logFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        Get-Content $latestLog.FullName -Tail 10 | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    }
    else {
        Write-Host "⚠ No log files found in logs directory" -ForegroundColor Yellow
    }
}
else {
    Write-Host "✗ Logs directory missing: $logsPath" -ForegroundColor Red
    Write-Host "Creating logs directory..." -ForegroundColor Yellow
    New-Item -Path $logsPath -ItemType Directory -Force | Out-Null
}

# Step 4: Check IIS logs
Write-Host "`n4. Checking IIS logs..." -ForegroundColor Yellow
$iisLogsPath = "C:\inetpub\logs\LogFiles\W3SVC1"
if (Test-Path $iisLogsPath) {
    $iisLogFiles = Get-ChildItem "$iisLogsPath\*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($iisLogFiles) {
        Write-Host "✓ Found IIS logs" -ForegroundColor Green
        Write-Host "`nRecent IIS log entries:" -ForegroundColor Cyan
        Get-Content $iisLogFiles.FullName -Tail 5 | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    }
}
else {
    Write-Host "⚠ IIS logs directory not found" -ForegroundColor Yellow
}

# Step 5: Test application manually
Write-Host "`n5. Testing application startup..." -ForegroundColor Yellow
try {
    Push-Location $AppPath
    Write-Host "Attempting to start application manually..." -ForegroundColor Cyan
    $testProcess = Start-Process -FilePath "dotnet" -ArgumentList "TeachSpark.Web.dll --urls=http://localhost:5555" -PassThru -NoNewWindow
    Start-Sleep -Seconds 3
    
    if (!$testProcess.HasExited) {
        Write-Host "✓ Application started successfully (PID: $($testProcess.Id))" -ForegroundColor Green
        Write-Host "Stopping test process..." -ForegroundColor Yellow
        $testProcess.Kill()
    }
    else {
        Write-Host "✗ Application failed to start" -ForegroundColor Red
    }
}
catch {
    Write-Host "✗ Error testing application: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Pop-Location
}

# Step 6: Check asset manifest
Write-Host "`n6. Checking asset manifest..." -ForegroundColor Yellow
$manifestPath = "$AppPath\wwwroot\assets-manifest.json"
if (Test-Path $manifestPath) {
    Write-Host "✓ Asset manifest exists" -ForegroundColor Green
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $assetCount = ($manifest | Get-Member -MemberType NoteProperty).Count
    Write-Host "✓ Found $assetCount assets in manifest" -ForegroundColor Green
}
else {
    Write-Host "✗ Asset manifest missing: $manifestPath" -ForegroundColor Red
}

# Step 7: Enable diagnostic mode if requested
if ($EnableDiagnosticMode) {
    Write-Host "`n7. Enabling diagnostic mode..." -ForegroundColor Yellow
    $diagnosticWebConfig = "$AppPath\web.config.diagnostic"
    if (Test-Path $diagnosticWebConfig) {
        Write-Host "Backing up current web.config..." -ForegroundColor Cyan
        Copy-Item "$AppPath\web.config" "$AppPath\web.config.backup" -Force
        Write-Host "Applying diagnostic web.config..." -ForegroundColor Cyan
        Copy-Item $diagnosticWebConfig "$AppPath\web.config" -Force
        Write-Host "✓ Diagnostic mode enabled" -ForegroundColor Green
        Write-Host "⚠ Remember to restore web.config.backup when done troubleshooting" -ForegroundColor Yellow
    }
    else {
        Write-Host "✗ Diagnostic web.config not found: $diagnosticWebConfig" -ForegroundColor Red
    }
}

Write-Host "`n=== Troubleshooting Complete ===" -ForegroundColor Red
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Check the application logs above for specific errors"
Write-Host "2. If logs are empty, the application may not be starting"
Write-Host "3. Use: .\Troubleshoot-TeachSpark.ps1 -EnableDiagnosticMode to get detailed errors"
Write-Host "4. Check https://teachspark.markhazleton.com for detailed error messages"
