#!/bin/bash

echo "🔄 Starting development services..."

# Check if SQL Server is running and accessible
echo "🗄️ Checking SQL Server connection..."
timeout=60
counter=0

while [ $counter -lt $timeout ]; do
    if sqlcmd -S localhost,1433 -U sa -P 'YourStrong@Passw0rd' -Q "SELECT 1" > /dev/null 2>&1; then
        echo "✅ SQL Server is ready"
        break
    fi
    
    echo "⏳ Waiting for SQL Server... ($counter/$timeout)"
    sleep 2
    counter=$((counter + 2))
done

if [ $counter -ge $timeout ]; then
    echo "⚠️ SQL Server connection timeout - database operations may fail"
else
    # Try to run any pending migrations
    if [ -d "/workspace/movie-price-api" ]; then
        echo "🔄 Checking for database migrations..."
        cd /workspace/movie-price-api
        dotnet ef database update --verbose 2>/dev/null || echo "⚠️ Migration check failed - may need manual intervention"
        cd /workspace
    fi
fi

# Display helpful information
echo ""
echo "🎯 Development Environment Ready!"
echo ""
echo "📋 Quick Start Commands:"
echo "  api          - Start the .NET API server"
echo "  frontend     - Start the React development server"
echo "  test-api     - Run backend unit tests"
echo "  test-frontend - Run frontend unit tests"
echo ""
echo "🔧 Database Commands:"
echo "  db-update    - Apply database migrations"
echo "  db-reset     - Reset database (drop and recreate)"
echo ""
echo "🌐 Service URLs:"
echo "  Frontend:    http://localhost:3000"
echo "  API:         https://localhost:7001"
echo "  SQL Server:  localhost:1433"
echo ""
echo "💡 Pro Tips:"
echo "  - Use 'Ctrl+Shift+\`' to open a new terminal"
echo "  - Run both 'api' and 'frontend' in separate terminals for full development"
echo "  - Check the README.md for detailed setup instructions"
echo ""
