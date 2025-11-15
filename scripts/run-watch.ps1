$ErrorActionPreference = "Stop"

$apiPath = Join-Path $PSScriptRoot "..\projects\api\src\MBC.Endpoints"
$webPath = Join-Path $PSScriptRoot "..\projects\web"

function Stop-DevelopmentProcesses {
    param(
        [System.Diagnostics.Process]$ApiProcess,
        [System.Diagnostics.Process]$WebProcess
    )
    
    Write-Host ""
    Write-Host "Cleaning up processes..." -ForegroundColor Yellow
    
    if ($ApiProcess -and -not $ApiProcess.HasExited) {
        $ApiProcess | Stop-Process -ErrorAction SilentlyContinue
        Write-Host "API process stopped." -ForegroundColor Gray
    }
    
    if ($WebProcess -and -not $WebProcess.HasExited) {
        $WebProcess | Stop-Process -ErrorAction SilentlyContinue
        Write-Host "Web process stopped." -ForegroundColor Gray
    }
    
    Write-Host "Processes stopped." -ForegroundColor Green
}

$apiProcess = $null
$webProcess = $null

$cleanup = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    Stop-DevelopmentProcesses -ApiProcess $apiProcess -WebProcess $webProcess
}

try {
    Write-Host "Starting Mango Bay Cargo - API and Web" -ForegroundColor Green
    Write-Host "=======================================" -ForegroundColor Green
    Write-Host ""

    if (-not (Test-Path $apiPath)) {
        Write-Host "Error: API path not found: $apiPath" -ForegroundColor Red
        exit 1
    }

    if (-not (Test-Path $webPath)) {
        Write-Host "Error: Web path not found: $webPath" -ForegroundColor Red
        exit 1
    }

    Write-Host "Starting .NET API in watch mode..." -ForegroundColor Cyan
    $apiProcess = Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$apiPath'; Write-Host 'API running on http://localhost:5110' -ForegroundColor Green; dotnet watch run" -PassThru

    Start-Sleep -Seconds 2

    Write-Host "Starting Angular Web in watch mode..." -ForegroundColor Cyan
    $webProcess = Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$webPath'; Write-Host 'Web running on http://localhost:4200' -ForegroundColor Green; npm start" -PassThru

    Write-Host ""
    Write-Host "Both processes started successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "API:  http://localhost:5110" -ForegroundColor Yellow
    Write-Host "Web:  http://localhost:4200" -ForegroundColor Yellow
    Write-Host ""

    Write-Host "Waiting for API to start..." -ForegroundColor Gray
    Start-Sleep -Seconds 6
    Write-Host "Opening Scalar API documentation..." -ForegroundColor Cyan
    Start-Process "http://localhost:5110/scalar/v1" -ErrorAction SilentlyContinue

    Write-Host "Waiting for Angular to compile..." -ForegroundColor Gray
    Start-Sleep -Seconds 8
    Write-Host "Opening web application..." -ForegroundColor Cyan
    Start-Process "http://localhost:4200/" -ErrorAction SilentlyContinue

    Write-Host ""
    Write-Host "Press any key to exit and stop all processes..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
finally {
    Unregister-Event -SourceIdentifier PowerShell.Exiting -ErrorAction SilentlyContinue
    Stop-DevelopmentProcesses -ApiProcess $apiProcess -WebProcess $webProcess
}
