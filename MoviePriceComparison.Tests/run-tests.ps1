#!/usr/bin/env pwsh

# Movie Price Comparison - Test Runner Script
# This script runs all unit tests with coverage reporting

Write-Host "🧪 Movie Price Comparison - Running Unit Tests" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 9.0 SDK" -ForegroundColor Red
    exit 1
}

# Navigate to test project directory
$testProjectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $testProjectPath

Write-Host ""
Write-Host "📁 Test Project: $testProjectPath" -ForegroundColor Yellow

# Restore packages
Write-Host ""
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed" -ForegroundColor Red
    exit 1
}

# Build the test project
Write-Host ""
Write-Host "🔨 Building test project..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}

# Run tests with coverage
Write-Host ""
Write-Host "🧪 Running unit tests with coverage..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage" --results-directory ./TestResults

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ All tests passed!" -ForegroundColor Green
    
    # Check if coverage files exist
    $coverageFiles = Get-ChildItem -Path "./TestResults" -Filter "coverage.cobertura.xml" -Recurse
    if ($coverageFiles.Count -gt 0) {
        Write-Host "📊 Coverage report generated: $($coverageFiles[0].FullName)" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "❌ Some tests failed!" -ForegroundColor Red
    exit 1
}

# Test summary
Write-Host ""
Write-Host "📋 Test Summary" -ForegroundColor Cyan
Write-Host "===============" -ForegroundColor Cyan
Write-Host "✅ Domain Layer Tests: Movie and MoviePrice entities" -ForegroundColor Green
Write-Host "✅ Application Layer Tests: Use cases and DTOs" -ForegroundColor Green
Write-Host "✅ Infrastructure Layer Tests: Repository implementations" -ForegroundColor Green
Write-Host "✅ Presentation Layer Tests: Controller endpoints" -ForegroundColor Green

Write-Host ""
Write-Host "🎯 Clean Architecture Testing Complete!" -ForegroundColor Green
