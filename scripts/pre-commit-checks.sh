#!/bin/bash
# Pre-commit checks for DotNet Ecuador API
# Bash script for Linux/macOS systems

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Parameters
SKIP_TESTS=false
SKIP_COVERAGE=false
VERBOSE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --skip-coverage)
            SKIP_COVERAGE=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        *)
            echo "Unknown parameter: $1"
            exit 1
            ;;
    esac
done

# Helper functions
print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

print_section() {
    echo ""
    print_color $BLUE "=================================================="
    print_color $BLUE "  $1"
    print_color $BLUE "=================================================="
    echo ""
}

print_success() {
    print_color $GREEN "✅ $1"
}

print_error() {
    print_color $RED "❌ $1"
}

print_warning() {
    print_color $YELLOW "⚠️  $1"
}

# Start pre-commit checks
print_section "DotNet Ecuador API - Pre-Commit Quality Checks"

EXIT_CODE=0
START_TIME=$(date +%s)

# Check if .NET is installed
print_section "Environment Verification"
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_success ".NET SDK Version: $DOTNET_VERSION"
else
    print_error ".NET SDK is not installed or not in PATH"
    exit 1
fi

# Restore packages
print_section "Package Restoration"
echo "Restoring NuGet packages..."
if dotnet restore --verbosity quiet; then
    print_success "Packages restored successfully"
else
    print_error "Package restoration failed"
    exit 1
fi

# Code formatting check
print_section "Code Formatting Verification"
echo "Checking code formatting..."
if dotnet format --verify-no-changes --verbosity diagnostic; then
    print_success "Code formatting is correct"
else
    print_error "Code formatting issues detected. Please run 'dotnet format' to fix them."
    exit 1
fi

# Build the solution
print_section "Solution Build"
echo "Building solution..."
if dotnet build --no-restore --configuration Release --verbosity quiet; then
    print_success "Solution built successfully"
else
    print_error "Build failed. Please fix compilation errors."
    exit 1
fi

# Static code analysis
print_section "Static Code Analysis"
echo "Running static code analysis..."
if dotnet build --no-restore --verbosity normal --configuration Release; then
    print_success "Static analysis completed"
else
    print_error "Static analysis found issues. Please review and fix warnings."
    EXIT_CODE=1
fi

# Run tests if not skipped
if [ "$SKIP_TESTS" = false ]; then
    print_section "Unit Tests Execution"
    echo "Running unit tests..."
    
    if [ "$SKIP_COVERAGE" = false ]; then
        # Run tests with coverage
        if dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"; then
            print_success "All tests passed"
        else
            print_error "Some tests failed. Please fix failing tests."
            exit 1
        fi
    else
        # Run tests without coverage
        if dotnet test --no-build --configuration Release --logger "console;verbosity=normal"; then
            print_success "All tests passed"
        else
            print_error "Some tests failed. Please fix failing tests."
            exit 1
        fi
    fi
    
    # Generate coverage report if not skipped
    if [ "$SKIP_COVERAGE" = false ]; then
        print_section "Code Coverage Analysis"
        echo "Generating coverage report..."
        
        # Find coverage files
        COVERAGE_FILE=$(find . -name "coverage.cobertura.xml" -type f | head -1)
        
        if [ -n "$COVERAGE_FILE" ]; then
            # Check if reportgenerator is available
            if command -v reportgenerator &> /dev/null; then
                reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html
                print_success "Coverage report generated in TestResults/CoverageReport"
            else
                print_warning "ReportGenerator not found. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
            fi
        else
            print_warning "No coverage files found"
        fi
    fi
else
    print_warning "Tests skipped by user request"
fi

# Security scan (if available)
print_section "Security Scan"
if command -v dotnet &> /dev/null; then
    if dotnet list package --vulnerable --include-transitive 2>/dev/null; then
        print_success "No vulnerable packages detected"
    else
        print_warning "Package vulnerability check completed with warnings"
    fi
else
    print_warning "Security scan not available"
fi

# File validation
print_section "File Validation"
echo "Checking for sensitive files..."

SENSITIVE_PATTERNS=("*.key" "*.pfx" "*.p12" ".env" "appsettings.Production.json" "*.secret" "*password*" "*secret*")
FOUND_SENSITIVE_FILES=()

for pattern in "${SENSITIVE_PATTERNS[@]}"; do
    while IFS= read -r -d '' file; do
        if [[ ! "$file" =~ test ]] && [[ ! "$file" =~ sample ]]; then
            FOUND_SENSITIVE_FILES+=("$file")
        fi
    done < <(find . -name "$pattern" -type f -print0 2>/dev/null)
done

if [ ${#FOUND_SENSITIVE_FILES[@]} -gt 0 ]; then
    print_warning "Potentially sensitive files found:"
    for file in "${FOUND_SENSITIVE_FILES[@]}"; do
        echo "  - $file"
    done
    print_warning "Please review these files before committing"
else
    print_success "No sensitive files detected"
fi

# Performance metrics
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))
MINUTES=$((DURATION / 60))
SECONDS=$((DURATION % 60))

# Summary
print_section "Pre-Commit Check Summary"
if [ $EXIT_CODE -eq 0 ]; then
    print_success "All quality checks passed! ✨"
else
    print_error "Some quality checks failed. Please fix the issues before committing."
fi

echo ""
printf "Total execution time: %02d:%02d\n" $MINUTES $SECONDS
echo ""

# Additional information
if [ "$VERBOSE" = true ]; then
    print_section "Additional Information"
    echo "- Code formatting: dotnet format"
    echo "- Manual build: dotnet build"
    echo "- Manual tests: dotnet test"
    echo "- Coverage: dotnet test --collect:'XPlat Code Coverage'"
    echo "- Security audit: dotnet list package --vulnerable"
fi

exit $EXIT_CODE