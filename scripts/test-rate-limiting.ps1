param(
    [string]$BaseUrl = "http://localhost:5110"
)

$endpoint = "$BaseUrl/api/v1/pilots"

Write-Host "`nTesting Rate Limiting against $endpoint" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host ""

$successCount = 0
$rateLimitedCount = 0
$errorCount = 0

function Invoke-RequestWithColor {
    param(
        [int]$RequestNumber,
        [string]$Url
    )
    
    try {
        $timestamp = Get-Date -Format "HH:mm:ss.fff"
        $response = Invoke-WebRequest -Uri $Url -Method GET -UseBasicParsing -ErrorAction SilentlyContinue
        
        if ($response.StatusCode -eq 200) {
            Write-Host "[$timestamp] Request $RequestNumber : " -NoNewline
            Write-Host "200 OK" -ForegroundColor Green -NoNewline
            Write-Host " ✓"
            return "success"
        }
    }
    catch {
        $timestamp = Get-Date -Format "HH:mm:ss.fff"
        if ($_.Exception.Response.StatusCode.value__ -eq 429) {
            Write-Host "[$timestamp] Request $RequestNumber : " -NoNewline
            Write-Host "429 Too Many Requests" -ForegroundColor Red -NoNewline
            Write-Host " ✗"
            return "rate-limited"
        }
        else {
            Write-Host "[$timestamp] Request $RequestNumber : " -NoNewline
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Yellow
            return "error"
        }
    }
}

Write-Host "Phase 1: Testing rate limit (expecting 5 successes, then failures)" -ForegroundColor Yellow
Write-Host ""

for ($i = 1; $i -le 15; $i++) {
    $result = Invoke-RequestWithColor -RequestNumber $i -Url $endpoint
    
    switch ($result) {
        "success" { $successCount++ }
        "rate-limited" { $rateLimitedCount++ }
        "error" { $errorCount++ }
    }
    
    Start-Sleep -Milliseconds 100
}

Write-Host ""
Write-Host "Phase 1 Summary:" -ForegroundColor Cyan
Write-Host "  Successful: $successCount" -ForegroundColor Green
Write-Host "  Rate Limited: $rateLimitedCount" -ForegroundColor Red
Write-Host "  Errors: $errorCount" -ForegroundColor Yellow

Write-Host ""
Write-Host "Waiting 11 seconds for rate limit window to expire..." -ForegroundColor Yellow

for ($i = 11; $i -gt 0; $i--) {
    Write-Host "  $i seconds remaining..." -NoNewline
    Start-Sleep -Seconds 1
    Write-Host "`r" -NoNewline
}

Write-Host "  Window expired!           " -ForegroundColor Green
Write-Host ""

Write-Host "Phase 2: Testing after window expiration (expecting successes again)" -ForegroundColor Yellow
Write-Host ""

$phase2Success = 0
$phase2RateLimited = 0
$phase2Error = 0

for ($i = 16; $i -le 20; $i++) {
    $result = Invoke-RequestWithColor -RequestNumber $i -Url $endpoint
    
    switch ($result) {
        "success" { $phase2Success++ }
        "rate-limited" { $phase2RateLimited++ }
        "error" { $phase2Error++ }
    }
    
    Start-Sleep -Milliseconds 100
}

Write-Host ""
Write-Host "Phase 2 Summary:" -ForegroundColor Cyan
Write-Host "  Successful: $phase2Success" -ForegroundColor Green
Write-Host "  Rate Limited: $phase2RateLimited" -ForegroundColor Red
Write-Host "  Errors: $phase2Error" -ForegroundColor Yellow

Write-Host ""
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "Overall Summary:" -ForegroundColor Cyan
Write-Host "  Total Successful: $($successCount + $phase2Success)" -ForegroundColor Green
Write-Host "  Total Rate Limited: $($rateLimitedCount + $phase2RateLimited)" -ForegroundColor Red
Write-Host "  Total Errors: $($errorCount + $phase2Error)" -ForegroundColor Yellow
Write-Host ""

if ($rateLimitedCount -gt 0 -and $phase2Success -gt 0) {
    Write-Host "✓ Rate limiting is working correctly!" -ForegroundColor Green
    Write-Host "  - Requests were blocked after hitting the limit" -ForegroundColor Green
    Write-Host "  - Requests succeeded after the window expired" -ForegroundColor Green
}
elseif ($rateLimitedCount -eq 0) {
    Write-Host "⚠ Rate limiting may not be working - no requests were blocked" -ForegroundColor Yellow
}
elseif ($phase2Success -eq 0) {
    Write-Host "⚠ Rate limit window may not be expiring correctly" -ForegroundColor Yellow
}

Write-Host ""

