#!/usr/bin/env pwsh

param(
    [switch]$SkipClean,
    [switch]$Build,
    [switch]$Verbose,
    [switch]$SkipOpen
)

$ErrorActionPreference = "Stop"

Write-Host "=== Code Coverage Report Generator ===" -ForegroundColor Cyan
Write-Host ""

$scriptPath = $PSScriptRoot
if (-not $scriptPath) {
    $scriptPath = Get-Location
}

$apiPath = Split-Path -Parent $scriptPath

Push-Location $apiPath
try {
    Write-Host "Ensuring local tools are installed..." -ForegroundColor Yellow
    $toolRestoreOutput = dotnet tool restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to restore dotnet tools. Output: $toolRestoreOutput"
        exit 1
    }
    Write-Host "✓ Tools ready" -ForegroundColor Green
    Write-Host ""

    if (-not $SkipClean) {
        Write-Host "Cleaning previous coverage data..." -ForegroundColor Yellow
        Remove-Item -Path "coverage-report" -Recurse -Force -ErrorAction SilentlyContinue
        Get-ChildItem -Path "tests" -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue | Remove-Item -Force
        Get-ChildItem -Path "tests" -Recurse -Filter "*.coverage" -ErrorAction SilentlyContinue | Remove-Item -Force
        Get-ChildItem -Path "tests" -Recurse -Directory -Filter "TestResults" -ErrorAction SilentlyContinue | ForEach-Object {
            Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
        }
        Write-Host "✓ Cleaned" -ForegroundColor Green
        Write-Host ""
    }

    if ($Build) {
        Write-Host "Building solution..." -ForegroundColor Yellow
        dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed!"
            exit 1
        }
        Write-Host "✓ Build complete" -ForegroundColor Green
        Write-Host ""
    }

    Write-Host "Running tests with coverage collection..." -ForegroundColor Yellow
    $verbosityArg = if ($Verbose) { "normal" } else { "minimal" }
    dotnet test --collect:"XPlat Code Coverage" --verbosity $verbosityArg

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed!"
        exit 1
    }
    Write-Host "✓ Tests complete" -ForegroundColor Green
    Write-Host ""

    Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow
    $coverageFiles = Get-ChildItem -Path "tests" -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue
    if ($coverageFiles.Count -eq 0) {
        Write-Error "No coverage files found! Make sure coverlet.collector is installed in your test projects."
        exit 1
    }

    Write-Host "Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Gray

    $coverageFilePaths = $coverageFiles | ForEach-Object { $_.FullName }
    $coverageFilesArg = $coverageFilePaths -join ";"

    dotnet reportgenerator `
        -reports:"$coverageFilesArg" `
        -targetdir:"coverage-report" `
        -reporttypes:"Html" `
        -assemblyfilters:"-*.Tests*" `
        -classfilters:"-*.Program;-MBC.Persistence.Migrations.*;-MBC.Services.Seeds.*" `
        -filefilters:"-*obj*;-*bin*;-*.g.cs;-*_g.cs;-*SourceGeneration*;-*Vogen*;-*TestResults*;-*.Designer.cs;-*.designer.cs;-GlobalUsings.g.cs;-AssemblyInfo.cs;-AssemblyAttributes.cs;-System.Text.Json.SourceGeneration*;-*JsonSourceGenerator*"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Report generation failed!"
        exit 1
    }
    Write-Host "✓ Report generated" -ForegroundColor Green
    Write-Host ""

    $reportPath = Join-Path -Path $apiPath -ChildPath "coverage-report\index.html"
    Write-Host "Coverage report location:" -ForegroundColor Cyan
    Write-Host "  $reportPath" -ForegroundColor White
    Write-Host ""

    if (-not $SkipOpen) {
        Write-Host "Opening coverage report in browser..." -ForegroundColor Yellow
        Start-Process $reportPath
        Write-Host "✓ Opened in browser" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "=== Coverage Report Complete ===" -ForegroundColor Cyan
}
finally {
    Pop-Location
}

