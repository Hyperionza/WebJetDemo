#!/bin/bash

echo "🚀 Setting up development environment..."

# Fix workspace permissions
echo "📁 Fixing workspace permissions..."
sudo chown -R vscode:vscode /workspace

# Clean any existing build artifacts that might have wrong permissions
echo "🧹 Cleaning build artifacts..."
find /workspace -name "bin" -type d -exec sudo rm -rf {} + 2>/dev/null || true
find /workspace -name "obj" -type d -exec sudo rm -rf {} + 2>/dev/null || true

# Clean and restore .NET packages
echo "📦 Cleaning and restoring .NET packages..."
cd /workspace/MoviePriceComparison
dotnet clean
dotnet restore --force
dotnet build --no-restore

# Install frontend dependencies
echo "📦 Installing frontend dependencies..."
cd /workspace/movie-price-frontend

# Clean any existing problematic installations
rm -rf node_modules package-lock.json 2>/dev/null || true

# Clear npm cache to avoid corruption issues
npm cache clean --force 2>/dev/null || true

# Simple install - let Create React App handle its own dependencies
npm install

# Add the one missing dependency that Create React App needs
npm install ajv@8.17.1

# Verify react-scripts is properly installed
if [ ! -f "node_modules/react-scripts/bin/react-scripts.js" ]; then
    echo "⚠️ react-scripts missing, something went wrong with the install"
    exit 1
fi

echo "✅ Frontend dependencies installed successfully (simple approach)"

# Create development configuration files if they don't exist
echo "⚙️ Setting up development configuration..."

# Backend configuration
if [ -d "MoviePriceComparison" ] && [ ! -f "MoviePriceComparison/appsettings.Development.json" ]; then
    cat > MoviePriceComparison/appsettings.Development.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Environment": "LOCALDEV",
  "ConnectionStrings": {
    "Redis": ""
  },
  "ExternalApis": {
    "ApiToken": "sjd1HfkjU83ksdsm3802k",
    "CinemaWorld": {
      "BaseUrl": "https://webjetapitest.azurewebsites.net/api/cinemaworld",
      "TokenKeyVaultName": "cinemaworld-api-token"
    },
    "FilmWorld": {
      "BaseUrl": "https://webjetapitest.azurewebsites.net/api/filmworld",
      "TokenKeyVaultName": "filmworld-api-token"
    }
  },
  "KeyVault": {
    "Url": ""
  },
  "ApiProviderService": {
    "ConfigurationServiceUrl": "http://localhost:5091/api/MockConfiguration/api-providers",
    "CacheDurationMinutes": 15,
    "TimeoutSeconds": 30
  }
}
EOF
    echo "✅ Created backend development configuration"
fi

# Frontend environment file
if [ -d "movie-price-frontend" ] && [ ! -f "movie-price-frontend/.env.development" ]; then
    cat > movie-price-frontend/.env.development << 'EOF'
REACT_APP_API_URL=http://localhost:5091
REACT_APP_ENVIRONMENT=development
GENERATE_SOURCEMAP=true
EOF
    echo "✅ Created frontend development configuration"
fi

# Set up Git hooks (if .git exists)
if [ -d ".git" ]; then
    echo "🔧 Setting up Git hooks..."
    
    # Pre-commit hook for code formatting
    cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
echo "Running pre-commit checks..."

# Check if there are any staged .cs files
if git diff --cached --name-only | grep -q '\.cs$'; then
    echo "Formatting .NET code..."
    dotnet format --include $(git diff --cached --name-only | grep '\.cs$' | tr '\n' ' ')
fi

# Check if there are any staged frontend files
if git diff --cached --name-only | grep -E '\.(ts|tsx|js|jsx)$'; then
    echo "Formatting frontend code..."
    cd movie-price-frontend
    npm run lint:fix 2>/dev/null || echo "⚠️ Frontend linting not available"
    cd ..
fi

echo "✅ Pre-commit checks completed"
EOF
    chmod +x .git/hooks/pre-commit
    echo "✅ Git hooks configured"
fi

# Create a convenient alias for testing the API
echo "🔧 Setting up development aliases..."
cat >> ~/.bashrc << 'EOF'

# Movie Price Comparison Development Aliases
alias api='cd /workspace/MoviePriceComparison && dotnet run'
alias frontend='cd /workspace/movie-price-frontend && npm start'
alias test-api='cd /workspace/MoviePriceComparison.Tests && dotnet test'
alias test-frontend='cd /workspace/movie-price-frontend && npm test'
alias build-all='cd /workspace && dotnet build && cd movie-price-frontend && npm run build'

# Dynamic API Provider Management
alias providers='curl -s http://localhost:5091/api/providers | jq'
alias refresh-providers='curl -X POST http://localhost:5091/api/providers/refresh'
alias mock-config='curl -s http://localhost:5091/api/MockConfiguration/api-providers | jq'
alias disable-provider='function _disable() { curl -X PATCH -H "Content-Type: application/json" -d "false" http://localhost:5091/api/MockConfiguration/api-providers/$1/status; }; _disable'
alias enable-provider='function _enable() { curl -X PATCH -H "Content-Type: application/json" -d "true" http://localhost:5091/api/MockConfiguration/api-providers/$1/status; }; _enable'

# Useful shortcuts and utilities
alias ll='ls -la'
alias workspace='cd /workspace'
alias fix-permissions='sudo chown -R vscode:vscode /workspace && find /workspace -name "bin" -type d -exec sudo rm -rf {} + 2>/dev/null || true && find /workspace -name "obj" -type d -exec sudo rm -rf {} + 2>/dev/null || true'
alias fix-frontend='cd /workspace && chmod +x fix-frontend-tests.sh && ./fix-frontend-tests.sh'
EOF
