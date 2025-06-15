# Development Container Setup

This directory contains the configuration for a complete development environment using VS Code Dev Containers. The setup provides a consistent, reproducible development environment for the Movie Price Comparison application.

## 🚀 Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/)
- [Dev Containers Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Getting Started
1. Clone the repository
2. Open VS Code in the project root
3. When prompted, click "Reopen in Container" or use `Ctrl+Shift+P` → "Dev Containers: Reopen in Container"
4. Wait for the container to build and setup to complete
5. Start developing!

## 🏗️ Container Architecture

### Services
- **app**: Main development container with .NET 9, Node.js 22, and development tools
- **redis**: for caching

### Included Tools
- **.NET 9 SDK**: For backend development
- **Node.js 22**: For frontend development
- **Docker CLI**: For container operations
- **GitHub CLI**: For repository operations
- **PowerShell**: For cross-platform scripting

### VS Code Extensions
The container automatically installs essential extensions:
- C# development tools
- TypeScript/React development tools
- Docker tools
- Git tools
- Testing tools

## 🔧 Configuration Files

### `devcontainer.json`
Main configuration file that defines:
- Container settings and features
- VS Code extensions to install
- Port forwarding configuration
- Environment variables
- Post-creation and post-start commands

### `docker-compose.yml`
Defines the multi-container setup:
- Development container with mounted workspace
- Redis configuration
- Network configuration
- Volume management

### `Dockerfile`
Custom container image with:
- Ubuntu base with development tools
- .NET 9 SDK installation
- Node.js 18 installation
- Development utilities

### Setup Scripts
- **`post-create.sh`**: Runs once when container is created
  - Restores .NET packages
  - Installs npm dependencies
  - Creates configuration files
  - Sets up Git hooks
  - Creates development aliases

- **`post-start.sh`**: Runs every time container starts
  - Displays helpful information

## 🌐 Port Configuration

| Port | Service | Description |
|------|---------|-------------|
| 3000 | React Frontend | Development server |
| 7001 | .NET API (HTTPS) | Backend API |
| 5000 | .NET API (HTTP) | Backend API |
| 6379 | Redis | Distributed cache provider |

## 📋 Development Commands

The container includes helpful aliases for common tasks:

### Application Commands
```bash
api          # Start the .NET API server
frontend     # Start the React development server
build-all    # Build both backend and frontend
```

### Testing Commands
```bash
test-api     # Run backend unit tests
test-frontend # Run frontend unit tests
```

### Utility Commands
```bash
workspace    # Navigate to workspace root
ll           # List files with details
fix-permissions # If there are strange things going on with the bin and obj folders
```

## 🛠️ Customization

### Adding Extensions
Edit the `extensions` array in `devcontainer.json`:
```json
"extensions": [
    "existing.extension",
    "new.extension.id"
]
```

### Adding Tools
Modify the `Dockerfile` to install additional tools:
```dockerfile
RUN apt-get update && apt-get install -y your-tool
```

### Environment Variables
Add to the `containerEnv` section in `devcontainer.json`:
```json
"containerEnv": {
    "YOUR_VARIABLE": "value"
}
```

## 🐛 Troubleshooting

### Container Won't Start
1. Ensure Docker Desktop is running
2. Check Docker Desktop has sufficient resources allocated
3. Try rebuilding the container: `Ctrl+Shift+P` → "Dev Containers: Rebuild Container"

### Port Conflicts
1. Check if ports 3000, 5000, 7001, or 6379 are in use locally
2. Stop conflicting services or modify port configuration

### Performance Issues
1. Increase Docker Desktop memory allocation (recommended: 8GB+)
2. Enable WSL 2 backend on Windows
3. Consider using volume mounts instead of bind mounts for better performance

## 📚 Additional Resources

- [VS Code Dev Containers Documentation](https://code.visualstudio.com/docs/remote/containers)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET in Docker](https://docs.microsoft.com/en-us/dotnet/core/docker/)

## 🤝 Contributing to Dev Container

When modifying the dev container configuration:
1. Test changes thoroughly
2. Update this documentation
3. Consider backward compatibility
4. Test on different platforms (Windows, macOS, Linux)
