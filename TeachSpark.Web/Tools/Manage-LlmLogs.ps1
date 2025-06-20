# LLM Log Management Script for TeachSpark
# Purpose: Monitor, analyze, and clean up LLM interaction log files

param(
    [string]$Action = "info", # info, list, clean, analyze
    [int]$Days = 30, # For clean action: days to keep
    [int]$Count = 10          # For list action: number of files to show
)

$LogPath = "c:\websites\teachspark\logs"

# Ensure log directory exists
if (-not (Test-Path $LogPath)) {
    Write-Host "Creating log directory: $LogPath" -ForegroundColor Yellow
    New-Item -Path $LogPath -ItemType Directory -Force | Out-Null
}

function Show-LogInfo {
    Write-Host "=== LLM Log Directory Information ===" -ForegroundColor Cyan
    Write-Host "Path: $LogPath" -ForegroundColor Gray
    
    $successLogs = Get-ChildItem -Path $LogPath -Filter "*_success.json" -ErrorAction SilentlyContinue
    $errorLogs = Get-ChildItem -Path $LogPath -Filter "*_error.json" -ErrorAction SilentlyContinue
    $totalLogs = $successLogs.Count + $errorLogs.Count
    
    Write-Host "Total log files: $totalLogs" -ForegroundColor White
    Write-Host "Successful calls: $($successLogs.Count)" -ForegroundColor Green
    Write-Host "Failed calls: $($errorLogs.Count)" -ForegroundColor Red
    
    if ($totalLogs -gt 0) {
        $oldestLog = ($successLogs + $errorLogs) | Sort-Object CreationTime | Select-Object -First 1
        $newestLog = ($successLogs + $errorLogs) | Sort-Object CreationTime -Descending | Select-Object -First 1
        
        Write-Host "Oldest log: $($oldestLog.CreationTime) - $($oldestLog.Name)" -ForegroundColor Gray
        Write-Host "Newest log: $($newestLog.CreationTime) - $($newestLog.Name)" -ForegroundColor Gray
        
        $totalSize = ($successLogs + $errorLogs) | Measure-Object -Property Length -Sum
        $sizeMB = [math]::Round($totalSize.Sum / 1MB, 2)
        Write-Host "Total size: $sizeMB MB" -ForegroundColor Gray
    }
}

function Show-RecentLogs {
    param([int]$Count)
    
    Write-Host "=== Recent LLM Logs (Last $Count) ===" -ForegroundColor Cyan
    
    $recentLogs = Get-ChildItem -Path $LogPath -Filter "LLM_*.json" -ErrorAction SilentlyContinue | 
    Sort-Object CreationTime -Descending | 
    Select-Object -First $Count
    
    if ($recentLogs.Count -eq 0) {
        Write-Host "No log files found." -ForegroundColor Yellow
        return
    }
    
    foreach ($log in $recentLogs) {
        $status = if ($log.Name -match "_success\.json$") { "✅ SUCCESS" } else { "❌ ERROR  " }
        $timestamp = $log.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
        $sizeKB = [math]::Round($log.Length / 1KB, 1)
        
        Write-Host "$status $timestamp $($log.Name) ($sizeKB KB)" -ForegroundColor $(if ($log.Name -match "_success\.json$") { "Green" } else { "Red" })
    }
}

function Remove-OldLogs {
    param([int]$DaysToKeep)
    
    Write-Host "=== Cleaning Old LLM Logs ===" -ForegroundColor Cyan
    Write-Host "Removing logs older than $DaysToKeep days..." -ForegroundColor Yellow
    
    $cutoffDate = (Get-Date).AddDays(-$DaysToKeep)
    $oldLogs = Get-ChildItem -Path $LogPath -Filter "LLM_*.json" -ErrorAction SilentlyContinue | 
    Where-Object { $_.CreationTime -lt $cutoffDate }
    
    if ($oldLogs.Count -eq 0) {
        Write-Host "No old logs found to remove." -ForegroundColor Green
        return
    }
    
    Write-Host "Found $($oldLogs.Count) old logs to remove:" -ForegroundColor Yellow
    foreach ($log in $oldLogs) {
        Write-Host "  - $($log.Name) ($($log.CreationTime))" -ForegroundColor Gray
    }
    
    $confirm = Read-Host "Remove these files? (y/N)"
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        $oldLogs | Remove-Item -Force
        Write-Host "Removed $($oldLogs.Count) old log files." -ForegroundColor Green
    }
    else {
        Write-Host "Cleanup cancelled." -ForegroundColor Yellow
    }
}

function Show-LogAnalysis {
    Write-Host "=== LLM Log Analysis ===" -ForegroundColor Cyan
    
    $allLogs = Get-ChildItem -Path $LogPath -Filter "LLM_*.json" -ErrorAction SilentlyContinue
    
    if ($allLogs.Count -eq 0) {
        Write-Host "No log files found for analysis." -ForegroundColor Yellow
        return
    }
    
    Write-Host "Analyzing $($allLogs.Count) log files..." -ForegroundColor Gray
    
    $stats = @{
        TotalCalls      = 0
        SuccessfulCalls = 0
        FailedCalls     = 0
        TotalTokens     = 0
        TotalCost       = 0
        WorksheetTypes  = @{}
        Models          = @{}
        Users           = @{}
    }
    
    foreach ($logFile in $allLogs) {
        try {
            $logContent = Get-Content $logFile.FullName -Raw | ConvertFrom-Json
            $stats.TotalCalls++
            
            if ($logContent.status -eq "SUCCESS") {
                $stats.SuccessfulCalls++
                
                if ($logContent.metadata.tokensUsed) {
                    $stats.TotalTokens += $logContent.metadata.tokensUsed
                }
                
                if ($logContent.metadata.cost) {
                    $stats.TotalCost += $logContent.metadata.cost
                }
                
                if ($logContent.request.worksheetType) {
                    $type = $logContent.request.worksheetType
                    $stats.WorksheetTypes[$type] = ($stats.WorksheetTypes[$type] ?? 0) + 1
                }
                
                if ($logContent.metadata.modelUsed) {
                    $model = $logContent.metadata.modelUsed
                    $stats.Models[$model] = ($stats.Models[$model] ?? 0) + 1
                }
                
                if ($logContent.metadata.userEmail) {
                    $user = $logContent.metadata.userEmail
                    $stats.Users[$user] = ($stats.Users[$user] ?? 0) + 1
                }
            }
            else {
                $stats.FailedCalls++
            }
        }
        catch {
            Write-Host "Warning: Could not parse $($logFile.Name)" -ForegroundColor Yellow
        }
    }
    
    # Display results
    Write-Host ""
    Write-Host "📊 Summary Statistics:" -ForegroundColor White
    Write-Host "  Total Calls: $($stats.TotalCalls)" -ForegroundColor Gray
    Write-Host "  Successful: $($stats.SuccessfulCalls) ($([math]::Round(($stats.SuccessfulCalls / $stats.TotalCalls) * 100, 1))%)" -ForegroundColor Green
    Write-Host "  Failed: $($stats.FailedCalls) ($([math]::Round(($stats.FailedCalls / $stats.TotalCalls) * 100, 1))%)" -ForegroundColor Red
    Write-Host "  Total Tokens: $($stats.TotalTokens)" -ForegroundColor Gray
    Write-Host "  Total Cost: $([math]::Round($stats.TotalCost, 4))" -ForegroundColor Gray
    
    if ($stats.WorksheetTypes.Count -gt 0) {
        Write-Host ""
        Write-Host "📋 Top Worksheet Types:" -ForegroundColor White
        $stats.WorksheetTypes.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 5 | ForEach-Object {
            Write-Host "  $($_.Key): $($_.Value)" -ForegroundColor Gray
        }
    }
    
    if ($stats.Models.Count -gt 0) {
        Write-Host ""
        Write-Host "🤖 Models Used:" -ForegroundColor White
        $stats.Models.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object {
            Write-Host "  $($_.Key): $($_.Value)" -ForegroundColor Gray
        }
    }
    
    if ($stats.Users.Count -gt 0) {
        Write-Host ""
        Write-Host "👥 Top Users:" -ForegroundColor White
        $stats.Users.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 5 | ForEach-Object {
            Write-Host "  $($_.Key): $($_.Value)" -ForegroundColor Gray
        }
    }
}

# Main execution
switch ($Action.ToLower()) {
    "info" { Show-LogInfo }
    "list" { Show-RecentLogs -Count $Count }
    "clean" { Remove-OldLogs -DaysToKeep $Days }
    "analyze" { Show-LogAnalysis }
    default {
        Write-Host "LLM Log Management Script" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Usage: .\Manage-LlmLogs.ps1 -Action <action> [options]" -ForegroundColor White
        Write-Host ""
        Write-Host "Actions:" -ForegroundColor Yellow
        Write-Host "  info     - Show log directory information (default)" -ForegroundColor Gray
        Write-Host "  list     - List recent log files" -ForegroundColor Gray
        Write-Host "  clean    - Remove old log files" -ForegroundColor Gray
        Write-Host "  analyze  - Analyze log data and show statistics" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Options:" -ForegroundColor Yellow
        Write-Host "  -Days    - Days to keep for clean action (default: 30)" -ForegroundColor Gray
        Write-Host "  -Count   - Number of files to show for list action (default: 10)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  .\Manage-LlmLogs.ps1" -ForegroundColor Gray
        Write-Host "  .\Manage-LlmLogs.ps1 -Action list -Count 20" -ForegroundColor Gray
        Write-Host "  .\Manage-LlmLogs.ps1 -Action clean -Days 7" -ForegroundColor Gray
        Write-Host "  .\Manage-LlmLogs.ps1 -Action analyze" -ForegroundColor Gray
    }
}
