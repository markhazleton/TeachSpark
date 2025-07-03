Write-Host "=== TeachSpark Worksheet Display Diagnostic ===" -ForegroundColor Cyan

# Check if the application is running
$webUrl = "https://teachspark.markhazleton.com"
Write-Host "`nTesting basic site connectivity..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri $webUrl -Method HEAD -TimeoutSec 10
    Write-Host "✓ Site is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
}
catch {
    Write-Host "✗ Site connectivity issue: $($_.Exception.Message)" -ForegroundColor Red
}

# Test the specific worksheet URL
$worksheetUrl = "$webUrl/WorksheetGenerator/Display/15"
Write-Host "`nTesting worksheet URL: $worksheetUrl" -ForegroundColor Yellow

try {
    $worksheetResponse = Invoke-WebRequest -Uri $worksheetUrl -Method HEAD -TimeoutSec 10 -ErrorAction SilentlyContinue
    Write-Host "✓ Worksheet URL accessible (Status: $($worksheetResponse.StatusCode))" -ForegroundColor Green
}
catch {
    Write-Host "✗ Worksheet URL error: $($_.Exception.Message)" -ForegroundColor Red
    
    # Check if it's a 500/520 error
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  Status Code: $statusCode" -ForegroundColor Red
        
        if ($statusCode -eq 500 -or $statusCode -eq 520) {
            Write-Host "  This indicates a server-side error. Check application logs." -ForegroundColor Yellow
        }
    }
}

# Check log files for recent errors
$logPath = "C:\websites\TeachSpark\logs"
Write-Host "`nChecking recent log files..." -ForegroundColor Yellow

if (Test-Path $logPath) {
    $latestLog = Get-ChildItem -Path $logPath -Filter "*.txt" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($latestLog) {
        Write-Host "Latest log file: $($latestLog.FullName)" -ForegroundColor Green
        Write-Host "Last modified: $($latestLog.LastWriteTime)" -ForegroundColor Green
        
        # Look for recent errors
        $recentErrors = Get-Content $latestLog.FullName | Select-String -Pattern "ERR|ERROR|FATAL|Exception" | Select-Object -Last 10
        
        if ($recentErrors) {
            Write-Host "`nRecent errors found:" -ForegroundColor Red
            $recentErrors | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        }
        else {
            Write-Host "No recent errors found in log file." -ForegroundColor Green
        }
        
        # Look for authentication/user-related logs
        Write-Host "`nChecking for authentication/user logs..." -ForegroundColor Yellow
        $userLogs = Get-Content $latestLog.FullName | Select-String -Pattern "User authenticated|UserId|Display.*15" | Select-Object -Last 5
        
        if ($userLogs) {
            Write-Host "Recent user/display logs:" -ForegroundColor Cyan
            $userLogs | ForEach-Object { Write-Host "  $_" -ForegroundColor Cyan }
        }
    }
    else {
        Write-Host "No log files found in $logPath" -ForegroundColor Red
    }
}
else {
    Write-Host "Log directory not found: $logPath" -ForegroundColor Red
}

# Check database file
$dbPath = "c:\websites\teachspark\teachspark.db"
Write-Host "`nChecking database..." -ForegroundColor Yellow

if (Test-Path $dbPath) {
    $dbInfo = Get-Item $dbPath
    Write-Host "✓ Database file exists: $($dbInfo.FullName)" -ForegroundColor Green
    Write-Host "  Size: $([math]::Round($dbInfo.Length / 1KB, 2)) KB" -ForegroundColor Green
    Write-Host "  Last modified: $($dbInfo.LastWriteTime)" -ForegroundColor Green
}
else {
    Write-Host "✗ Database file not found: $dbPath" -ForegroundColor Red
}

Write-Host "`n=== Diagnostic Complete ===" -ForegroundColor Cyan
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. If 500/520 error persists, check application logs for authentication issues" -ForegroundColor White
Write-Host "2. Verify user is logged in and session is valid" -ForegroundColor White
Write-Host "3. Check if worksheet ID 15 exists and belongs to the current user" -ForegroundColor White
Write-Host "4. Consider adding debug logging to the Display action" -ForegroundColor White
