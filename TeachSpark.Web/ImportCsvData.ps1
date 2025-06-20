# PowerShell script to import CSV data via web API
$csvFilePath = "c:\GitHub\MarkHazleton\TeachSpark\TeachSpark.Web\Data\2020cas-rw.csv"
$url = "http://localhost:5213/Admin/AcademicStandards/ImportFromFile"

# Make API call
try {
    $body = @{
        filePath = $csvFilePath
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
    
    if ($response.success) {
        Write-Host "Success: $($response.message)" -ForegroundColor Green
        Write-Host "Imported $($response.count) records" -ForegroundColor Green
    }
    else {
        Write-Host "Error: $($response.message)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error calling API: $_" -ForegroundColor Red
}
