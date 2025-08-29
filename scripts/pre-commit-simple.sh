#!/bin/bash
# Simplified pre-commit checks for initial setup

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_color() {
    echo -e "${1}${2}${NC}"
}

print_success() {
    print_color $GREEN "✅ $1"
}

print_error() {
    print_color $RED "❌ $1"
}

print_info() {
    print_color $BLUE "ℹ️  $1"
}

print_info "Running simplified pre-commit checks..."

# Check .NET availability
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found"
    exit 1
fi

# Restore packages
print_info "Restoring packages..."
if dotnet restore --verbosity quiet; then
    print_success "Packages restored"
else
    print_error "Package restore failed"
    exit 1
fi

# Build solution
print_info "Building solution..."
BUILD_OUTPUT=$(dotnet build --no-restore --configuration Release --verbosity normal 2>&1 || true)

# Check if there are actual errors (not just warnings)
if echo "$BUILD_OUTPUT" | grep -i "error CS\|error MSB\|Build FAILED" | grep -v "warning" > /dev/null; then
    print_error "Build failed with errors"
    echo "$BUILD_OUTPUT" | grep -i "error CS\|error MSB"
    exit 1
else
    # Count warnings
    WARNING_COUNT=$(echo "$BUILD_OUTPUT" | grep -i "warning" | wc -l || echo "0")
    if [ "$WARNING_COUNT" -gt 0 ]; then
        print_color $YELLOW "⚠️  Build completed with $WARNING_COUNT warning(s)"
        print_info "Warnings won't block commit - continuing..."
    fi
    print_success "Build successful"
fi

# Run tests
print_info "Running tests..."
if dotnet test --no-build --configuration Release --verbosity quiet; then
    print_success "All tests passed"
else
    print_error "Some tests failed"
    exit 1
fi

print_success "Pre-commit checks completed successfully! ✨"