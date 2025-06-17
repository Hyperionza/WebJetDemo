#!/bin/bash

echo "🔧 Fixing frontend environment (simple approach)..."

# Navigate to frontend directory
cd /workspace/movie-price-frontend

# Clean existing node_modules and package-lock.json
echo "🧹 Cleaning existing dependencies..."
rm -rf node_modules package-lock.json

# Clear npm cache
echo "🗑️ Clearing npm cache..."
npm cache clean --force

# Simple install - let Create React App handle its own dependencies
echo "📦 Installing dependencies (no overrides, no flags)..."
npm install

# Add the one missing dependency that Create React App needs
echo "🔧 Adding missing ajv dependency..."
npm install ajv@8.17.1

# Verify react-scripts installation
echo "✅ Verifying react-scripts installation..."
if [ -f "node_modules/react-scripts/bin/react-scripts.js" ]; then
    echo "✅ react-scripts is properly installed"
else
    echo "❌ react-scripts is missing, something went wrong"
    exit 1
fi

# Test the installation
echo "🧪 Testing frontend setup..."
npm test -- --watchAll=false --passWithNoTests

echo "✅ Frontend environment fixed with simple approach!"
echo "🎉 You can now run 'frontend' to start the dev server!"
