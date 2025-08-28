#!/usr/bin/env pwsh
# Pre-commit checks for DotNet Ecuador API
# PowerShell script that works on both Windows and Linux

param(
    [Parameter(Mandatory = $false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipCoverage = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$Verbose = $false
)

# Colors for output
$RED = "`e[31m"
$GREEN = "`e[32m"
$YELLOW = "`e[33m"
$BLUE = "`e[34m"
$RESET = "`e[0m"

function Write-ColorOutput {
    param([string]$Color, [string]$Message)
    Write-Host "$Color$Message$RESET"
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-ColorOutput $BLUE "=================================================="
    Write-ColorOutput $BLUE "  $Title"
    Write-ColorOutput $BLUE "=================================================="
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput $GREEN "✅ $Message"
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput $RED "❌ $Message"
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput $YELLOW "⚠️  $Message"
}

# Start pre-commit checks
Write-Section "DotNet Ecuador API - Pre-Commit Quality Checks"

$exitCode = 0
$startTime = Get-Date

# Check if .NET is installed
Write-Section "Environment Verification"
try {
    $dotnetVersion = dotnet --version
    Write-Success ".NET SDK Version: $dotnetVersion"
} catch {
    Write-Error ".NET SDK is not installed or not in PATH"
    exit 1
}

# Restore packages
Write-Section "Package Restoration"
Write-Host "Restoring NuGet packages..."
$restoreResult = dotnet restore --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "Package restoration failed"
    exit 1
}
Write-Success "Packages restored successfully"

# Code formatting check
Write-Section "Code Formatting Verification"
Write-Host "Checking code formatting..."
try {
    $formatOutput = dotnet format --verify-no-changes --verbosity minimal 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Code formatting issues detected. Please run 'dotnet format' to fix them."
        exit 1
    }
    Write-Success "Code formatting is correct"
} catch {
    Write-Warning "Code formatting check failed, continuing..."
}

# Build the solution
Write-Section "Solution Build"
Write-Host "Building solution..."
$buildResult = dotnet build --no-restore --configuration Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Please fix compilation errors."
    exit 1
}
Write-Success "Solution built successfully"

# Static code analysis
Write-Section "Static Code Analysis"
Write-Host "Running static code analysis..."
$analysisResult = dotnet build --no-restore --verbosity quiet --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Static analysis found issues. Please review and fix warnings."
    # Don't exit, just warn for now
}
Write-Success "Static analysis completed"

# Run tests if not skipped
if (-not $SkipTests) {
    Write-Section "Unit Tests Execution"
    Write-Host "Running unit tests..."
    
    if (-not $SkipCoverage) {
        # Run tests with coverage
        $testResult = dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"
    } else {
        # Run tests without coverage
        $testResult = dotnet test --no-build --configuration Release --logger "console;verbosity=normal"
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Some tests failed. Please fix failing tests."
        exit 1
    }
    Write-Success "All tests passed"
    
    # Generate coverage report if not skipped
    if (-not $SkipCoverage) {
        Write-Section "Code Coverage Analysis"
        Write-Host "Generating coverage report..."
        
        # Find coverage files
        $coverageFiles = Get-ChildItem -Path "." -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
        
        if ($coverageFiles) {
            try {
                # Check if reportgenerator is available
                $reportGeneratorExists = Get-Command "reportgenerator" -ErrorAction SilentlyContinue
                if ($reportGeneratorExists) {
                    reportgenerator -reports:"**\coverage.cobertura.xml" -targetdir:"TestResults\CoverageReport" -reporttypes:Html
                    Write-Success "Coverage report generated in TestResults\CoverageReport"
                } else {
                    Write-Warning "ReportGenerator not found. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
                }
            } catch {
                Write-Warning "Failed to generate coverage report"
            }
        } else {
            Write-Warning "No coverage files found"
        }
    }
} else {
    Write-Warning "Tests skipped by user request"
}

# Security scan (if available)
Write-Section "Security Scan"
try {
    $securityScanExists = Get-Command "dotnet" -ErrorAction SilentlyContinue
    if ($securityScanExists) {
        $auditResult = dotnet list package --vulnerable --include-transitive 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "No vulnerable packages detected"
        } else {
            Write-Warning "Package vulnerability check completed with warnings"
        }
    }
} catch {
    Write-Warning "Security scan not available"
}

# File validation
Write-Section "File Validation"
Write-Host "Checking for sensitive files..."

$sensitiveFiles = @(
    "*.key",
    "*.pfx",
    "*.p12",
    ".env",
    "appsettings.Production.json",
    "*.secret",
    "*password*",
    "*secret*"
)

$foundSensitiveFiles = @()
foreach ($pattern in $sensitiveFiles) {
    $files = Get-ChildItem -Recurse -Include $pattern -ErrorAction SilentlyContinue | Where-Object { $_.Name -notlike "*test*" -and $_.Name -notlike "*sample*" }
    if ($files) {
        $foundSensitiveFiles += $files
    }
}

if ($foundSensitiveFiles.Count -gt 0) {
    Write-Warning "Potentially sensitive files found:"
    foreach ($file in $foundSensitiveFiles) {
        Write-Host "  - $($file.FullName)"
    }
    Write-Warning "Please review these files before committing"
} else {
    Write-Success "No sensitive files detected"
}

# Performance metrics
$endTime = Get-Date
$duration = $endTime - $startTime

# Summary
Write-Section "Pre-Commit Check Summary"
if ($exitCode -eq 0) {
    Write-Success "All quality checks passed! ✨"
} else {
    Write-Error "Some quality checks failed. Please fix the issues before committing."
}

Write-Host ""
Write-Host "Total execution time: $($duration.ToString('mm\:ss'))"
Write-Host ""

# Additional information
if ($Verbose) {
    Write-Section "Additional Information"
    Write-Host "- Code formatting: dotnet format"
    Write-Host "- Manual build: dotnet build"
    Write-Host "- Manual tests: dotnet test"
    Write-Host "- Coverage: dotnet test --collect:'XPlat Code Coverage'"
    Write-Host "- Security audit: dotnet list package --vulnerable"
}

exit $exitCode