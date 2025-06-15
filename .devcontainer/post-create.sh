#!/bin/bash

echo "🚀 Setting up Movie Price Comparison development environment..."

# Make sure we're in the workspace directory
cd /workspace

# Install .NET dependencies and restore packages
echo "📦 Restoring .NET packages..."
if [ -f "WebJet.sln" ]; then
    dotnet restore WebJet.sln
fi

# Update npm to latest version
echo "🔄 Updating npm to latest version..."
npm install -g npm@latest

# Install frontend dependencies
echo "📦 Installing frontend dependencies..."
if [ -d "movie-price-frontend" ]; then
    cd movie-price-frontend
    npm install
    cd ..
fi

# Set up Entity Framework database
echo "🗄️ Setting up database..."
if [ -d "MoviePriceComparison" ]; then
    cd MoviePriceComparison
    # Wait for SQL Server to be ready
    echo "⏳ Waiting for SQL Server to be ready..."
    sleep 30
    
    # Create and run migrations
    dotnet ef database update --verbose || echo "⚠️ Database migration failed - will retry on first run"
    cd ..
fi

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
    "DefaultConnection": "Data Source=movies.db",
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

# Create useful aliases
echo "🔧 Setting up development aliases..."
cat >> ~/.bashrc << 'EOF'

# Movie Price Comparison Development Aliases
alias api='cd /workspace/MoviePriceComparison && dotnet run'
alias frontend='cd /workspace/movie-price-frontend && npm start'
alias test-api='cd /workspace/MoviePriceComparison.Tests && dotnet test'
alias test-frontend='cd /workspace/movie-price-frontend && npm test'
alias build-all='cd /workspace && dotnet build && cd movie-price-frontend && npm run build'
alias db-update='cd /workspace/MoviePriceComparison && dotnet ef database update'
alias db-reset='cd /workspace/MoviePriceComparison && dotnet ef database drop -f && dotnet ef database update'

# Dynamic API Provider Management
alias providers='curl -s http://localhost:5091/api/providers | jq'
alias refresh-providers='curl -X POST http://localhost:5091/api/providers/refresh'
alias mock-config='curl -s http://localhost:5091/api/MockConfiguration/api-providers | jq'
alias disable-provider='function _disable() { curl -X PATCH -H "Content-Type: application/json" -d "false" http://localhost:5091/api/MockConfiguration/api-providers/$1/status; }; _disable'
alias enable-provider='function _enable() { curl -X PATCH -H "Content-Type: application/json" -d "true" http://localhost:5091/api/MockConfiguration/api-providers/$1/status; }; _enable'

# Useful shortcuts
alias ll='ls -la'
alias workspace='cd /workspace'
EOF

echo "🎉 Development environment setup complete!"
echo ""
echo "📋 Available commands:"
echo "  api                    - Start the .NET API"
echo "  frontend               - Start the React frontend"
echo "  test-api               - Run backend tests"
echo "  test-frontend          - Run frontend tests"
echo "  build-all              - Build both backend and frontend"
echo "  db-update              - Update database with migrations"
echo "  db-reset               - Reset database (drop and recreate)"
echo ""
echo "🔧 Dynamic API Provider commands:"
echo "  providers              - View current API providers"
echo "  refresh-providers      - Refresh provider cache"
echo "  mock-config            - View mock configuration service"
echo "  disable-provider <id>  - Disable a provider (e.g., disable-provider filmworld)"
echo "  enable-provider <id>   - Enable a provider (e.g., enable-provider filmworld)"
echo ""
echo "🌐 Default URLs:"
echo "  Frontend: http://localhost:3000"
echo "  API: http://localhost:5091"
echo "  Swagger: http://localhost:5091/swagger"
echo "  Mock Config: http://localhost:5091/api/MockConfiguration/api-providers"
echo "  SQL Server: localhost:1433"
echo ""
echo "💡 Tips:"
echo "  - Run 'source ~/.bashrc' to load the new aliases in your current session"
echo "  - Use 'providers' to see current API provider configurations"
echo "  - Use 'disable-provider cinemaworld' to disable a provider for testing"
echo "  - Check the dynamic API provider documentation in MoviePriceComparison/README-DYNAMIC-API-PROVIDERS.md"
